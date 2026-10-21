using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Logging;
using TeaPie.Http.Parsing;
using TeaPie.Variables;

namespace TeaPie.Templating;

internal sealed partial class TemplateExpander(
    ILoopBlockScanner scanner,
    ILoopBodyMasker masker,
    ICollectionSourceResolver sourceResolver,
    IVariablesFluidModelBuilder modelBuilder,
    IVariables variables,
    TemplatingLimits limits,
    ILogger<TemplateExpander> logger) : ITemplateExpander
{
    private const string SourceAliasPrefix = "__teapie_loop_source_";
    private const string LoopIndexPrefix = "__teapie_loop_index_";
    private const string LoopLengthPrefix = "__teapie_loop_length_";
    private const string LoopOffsetPrefix = "__teapie_loop_offset_";
    private const string ContinueOffset = "continue";
    private const string GuardTargetName = "__teapie_loop_guard";
    private const string GuardFilterName = "__teapie_guard_loop_source";
    private const string TreeStartMarkerPrefix = "\u0000__teapie_loop_tree_start_";
    private const string TreeEndMarkerPrefix = "\u0000__teapie_loop_tree_end_";
    private const string MarkerSuffix = "__\u0000";

    private static readonly FluidParser Parser = new();

    public string Expand(string content, string filePath)
    {
        if (!TemplateDetector.ContainsTemplateTag(content))
        {
            WarnAboutUnsupportedTagDelimiter(content, filePath);
            return content;
        }

        var blocks = FindLoopBlocks(scanner, content, filePath);
        var forSources = ParseForTagSources(blocks, filePath);
        var isDynamic = Enumerable.Range(0, blocks.Count).Select(i => IsDynamicSource(i, blocks, forSources)).ToArray();

        var model = modelBuilder.Build(variables);
        var edits = new List<TextEdit>();
        var sources = ResolveSourcesAndCollectSourceEdits(blocks, forSources, isDynamic, model, filePath, edits);
        AddDynamicSourceGuardEdits(edits, content, blocks, forSources, isDynamic);

        var topLevelIndices = FindTopLevelIndices(blocks);
        AddTopLevelLoopMarkerEdits(edits, content, blocks, topLevelIndices);

        edits.AddRange(masker.FindMaskEdits(content, blocks));

        var transformed = TextEditApplier.Apply(content, edits, out var positionMap);
        var template = ParseTemplate(transformed, content, positionMap, filePath);

        var usage = new DynamicSourceUsage[blocks.Count];
        var context = BuildRenderContext(blocks, forSources, sources, model, usage, filePath);
        var rendered = RenderTemplate(template, context, filePath);
        var warnedIndices = WarnAboutDynamicSourcesEmptyForEveryOuterItem(forSources, usage, filePath);

        ValidateTopLevelLoopRequestCounts(rendered, blocks, forSources, topLevelIndices, warnedIndices, filePath);

        return StripTopLevelLoopMarkers(rendered, topLevelIndices);
    }

    private void WarnAboutUnsupportedTagDelimiter(string content, string filePath)
    {
        if (TemplateDetector.ContainsTagDelimiter(content))
        {
            logger.LogWarning(
                "Requests file at '{Path}' contains '{{%' but no supported template tag ({Tags}), so it is sent " +
                "unchanged. If it was meant to be a template, check the spelling of its tags.",
                filePath,
                TemplateDetector.SupportedTagNames);
        }
    }

    private static ForTagSource[] ParseForTagSources(IReadOnlyList<LoopBlock> blocks, string filePath)
    {
        try
        {
            return blocks.Select(block => ForTagSourceParser.Parse(block.SourceExpression)).ToArray();
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException(
                $"Templating error in '{filePath}': {StripTemplatingErrorPrefix(ex.Message)}", ex);
        }
    }

    private LoopSource?[] ResolveSourcesAndCollectSourceEdits(
        IReadOnlyList<LoopBlock> blocks, IReadOnlyList<ForTagSource> forSources, bool[] isDynamic,
        IReadOnlyDictionary<string, object?> model, string filePath, List<TextEdit> edits)
    {
        var sources = new LoopSource?[blocks.Count];

        for (var i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            var forSource = forSources[i];

            if (isDynamic[i])
            {
                AddSourceEdit(edits, block, forSource.ToFluidSource(forSource.CollectionExpression));
                continue;
            }

            var source = ResolveSource(sourceResolver, forSource.CollectionExpression, model, filePath);
            sources[i] = source;

            if (source.ItemCount == 0)
            {
                throw new InvalidOperationException(
                    $"Templating error in '{filePath}': loop over '{forSource.CollectionExpression}' produced " +
                    "zero items.");
            }

            var expandedCount = forSource.ApplyLiteralWindow(source.ItemCount);
            if (LoopBlockHierarchy.IsStandaloneBlock(i, blocks) && expandedCount > limits.MaxExpandedRequests)
            {
                throw new InvalidOperationException(
                    $"Templating error in '{filePath}': loop over '{forSource.CollectionExpression}' would expand " +
                    $"to {expandedCount} requests, exceeding the maximum of {limits.MaxExpandedRequests}.");
            }

            var collection = source.Collection is not null ? $"{SourceAliasPrefix}{i}" : forSource.CollectionExpression;
            AddSourceEdit(edits, block, forSource.ToFluidSource(collection));
        }

        return sources;
    }

    private static void AddSourceEdit(List<TextEdit> edits, LoopBlock block, string fluidSource)
    {
        if (!string.Equals(fluidSource, block.SourceExpression, StringComparison.Ordinal))
        {
            edits.Add(new TextEdit(block.SourceExpressionStartIndex, block.SourceExpressionRawLength, fluidSource));
        }
    }

    private static void AddDynamicSourceGuardEdits(
        List<TextEdit> edits, string content, IReadOnlyList<LoopBlock> blocks, IReadOnlyList<ForTagSource> forSources,
        bool[] isDynamic)
    {
        var ancestorsByBlock = Enumerable.Range(0, blocks.Count)
            .Select(i => LoopBlockHierarchy.GetAncestorIndices(i, blocks))
            .ToArray();

        for (var i = 0; i < blocks.Count; i++)
        {
            var leadsToDynamicSource = isDynamic[i]
                || Enumerable.Range(0, blocks.Count).Any(d => isDynamic[d] && ancestorsByBlock[d].Contains(i));

            if (ancestorsByBlock[i].Count == 0 || !leadsToDynamicSource)
            {
                continue;
            }

            var block = blocks[i];
            var parentIndex = ancestorsByBlock[i][0];
            var parent = forSources[parentIndex];
            var trimMarker = content[block.StartIndex + 2] == '-' ? "-" : string.Empty;
            var injected = $"{{%{trimMarker} assign {LoopIndexPrefix}{parentIndex} = forloop.index0 %}}";

            if (parent.IsReversed)
            {
                injected += $"{{% assign {LoopLengthPrefix}{parentIndex} = forloop.length %}}";
            }

            if (parent.IsReversed && HasEvaluableOffset(parent))
            {
                injected += $"{{% assign {LoopOffsetPrefix}{parentIndex} = {parent.OffsetExpression} %}}";
            }

            if (isDynamic[i])
            {
                injected += $"{{% assign {GuardTargetName} = {forSources[i].CollectionExpression} | " +
                    $"{GuardFilterName}: {i} %}}";
            }

            edits.Add(new TextEdit(block.StartIndex, 0, injected));
        }
    }

    private static List<int> FindTopLevelIndices(IReadOnlyList<LoopBlock> blocks)
        => Enumerable.Range(0, blocks.Count).Where(i => LoopBlockHierarchy.IsTopLevel(i, blocks)).ToList();

    private static void AddTopLevelLoopMarkerEdits(
        List<TextEdit> edits, string content, IReadOnlyList<LoopBlock> blocks, IReadOnlyList<int> topLevelIndices)
    {
        foreach (var rootIndex in topLevelIndices)
        {
            var block = blocks[rootIndex];
            var endIndex = block.StartIndex + block.Length;
            edits.Add(new TextEdit(
                FindStartMarkerIndex(content, block.StartIndex), 0, $"{TreeStartMarkerPrefix}{rootIndex}{MarkerSuffix}"));
            edits.Add(new TextEdit(
                FindEndMarkerIndex(content, endIndex), 0, $"{TreeEndMarkerPrefix}{rootIndex}{MarkerSuffix}"));
        }
    }

    // A marker must sit outside the whitespace that '{%-' / '-%}' trims, otherwise the trim stops at the marker.
    // When the whitespace after '-%}' is followed by '{%-', that tag trims it, so the end marker stays at the tag
    // and does not end up between the whitespace and a start marker moved in front of it.
    private static int FindStartMarkerIndex(string content, int tagStartIndex)
    {
        if (content[tagStartIndex + 2] != '-')
        {
            return tagStartIndex;
        }

        var index = tagStartIndex;
        while (index > 0 && char.IsWhiteSpace(content[index - 1]))
        {
            index--;
        }

        return index;
    }

    private static int FindEndMarkerIndex(string content, int tagEndIndex)
    {
        if (content[tagEndIndex - 3] != '-')
        {
            return tagEndIndex;
        }

        var index = tagEndIndex;
        while (index < content.Length && char.IsWhiteSpace(content[index]))
        {
            index++;
        }

        return string.CompareOrdinal(content, index, "{%-", 0, 3) == 0 ? tagEndIndex : index;
    }

    private static IFluidTemplate ParseTemplate(
        string transformed, string originalContent, IReadOnlyList<TransformedTextSpan> positionMap, string filePath)
    {
        if (!Parser.TryParse(transformed, out var template, out var parseError))
        {
            var originalError =
                FluidParseErrorMapper.RemapToOriginal(parseError, originalContent, transformed, positionMap);
            throw new InvalidOperationException(
                $"Templating error in '{filePath}': failed to parse template: {originalError}. If this file " +
                "contains literal '{{%' text that is not a TeaPie template tag, wrap it in " +
                "'{{% raw %}}...{{% endraw %}}'.");
        }

        return template;
    }

    private TemplateContext BuildRenderContext(
        IReadOnlyList<LoopBlock> blocks, IReadOnlyList<ForTagSource> forSources, LoopSource?[] sources,
        IReadOnlyDictionary<string, object?> variablesModel, DynamicSourceUsage[] usage, string filePath)
    {
        var options = CreateMemberAccessOptions(filePath);
        options.MaxSteps = limits.MaxRenderSteps;
        options.Filters.AddFilter(GuardFilterName, (input, arguments, context) =>
            GuardDynamicSource(input, arguments, context, blocks, forSources, usage, filePath, logger));

        var model = new Dictionary<string, object?>(variablesModel);
        for (var i = 0; i < blocks.Count; i++)
        {
            if (sources[i] is { Collection: not null } source)
            {
                model[$"{SourceAliasPrefix}{i}"] = source.Collection;
            }
        }

        return new TemplateContext(model, options);
    }

    private static ValueTask<FluidValue> GuardDynamicSource(
        FluidValue input, FilterArguments arguments, TemplateContext context, IReadOnlyList<LoopBlock> blocks,
        IReadOnlyList<ForTagSource> forSources, DynamicSourceUsage[] usage, string filePath, ILogger logger)
    {
        var blockIndex = (int)arguments.At(0).ToNumberValue();
        var forSource = forSources[blockIndex];
        var raw = input.ToObjectValue();

        if (raw is not IEnumerable enumerable || raw is string)
        {
            var outerItemPath = FormatOuterItemPath(blockIndex, blocks, forSources, context);
            throw new InvalidOperationException(
                $"Templating error in '{filePath}': variable '{forSource.CollectionExpression}' referenced in a " +
                $"'{{% for %}}' loop must be a collection ({outerItemPath}).");
        }

        usage[blockIndex].Evaluated++;

        if (enumerable.Cast<object?>().Any())
        {
            usage[blockIndex].NonEmpty++;
            return new ValueTask<FluidValue>(NilValue.Instance);
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Loop over '{Source}' in '{Path}' produced zero items for {OuterItem}, so it generated no requests " +
                "there.",
                forSource.CollectionExpression,
                filePath,
                FormatOuterItemPath(blockIndex, blocks, forSources, context));
        }

        return new ValueTask<FluidValue>(NilValue.Instance);
    }

    private HashSet<int> WarnAboutDynamicSourcesEmptyForEveryOuterItem(
        IReadOnlyList<ForTagSource> forSources, DynamicSourceUsage[] usage, string filePath)
    {
        var warnedIndices = new HashSet<int>();

        for (var i = 0; i < usage.Length; i++)
        {
            if (usage[i].Evaluated > 0 && usage[i].NonEmpty == 0)
            {
                warnedIndices.Add(i);
                logger.LogWarning(
                    "Loop over '{Source}' in '{Path}' produced zero items for every one of its {Count} outer " +
                    "item(s), so it generated no requests at all. Check that the collection is filled as expected.",
                    forSources[i].CollectionExpression,
                    filePath,
                    usage[i].Evaluated);
            }
        }

        return warnedIndices;
    }

    private struct DynamicSourceUsage
    {
        public int Evaluated;
        public int NonEmpty;
    }

    private static string FormatOuterItemPath(
        int blockIndex, IReadOnlyList<LoopBlock> blocks, IReadOnlyList<ForTagSource> forSources,
        TemplateContext context)
    {
        var ancestors = LoopBlockHierarchy.GetAncestorIndices(blockIndex, blocks);
        ancestors.Reverse();

        return string.Join('.', ancestors.Select(ancestorIndex =>
        {
            var position = ResolveSourcePosition(ancestorIndex, forSources[ancestorIndex], context);
            return $"{blocks[ancestorIndex].LoopVariableName}[{position.ToString(CultureInfo.InvariantCulture)}]";
        }));
    }

    private static decimal ResolveSourcePosition(int blockIndex, ForTagSource forSource, TemplateContext context)
    {
        var index = context.GetValue($"{LoopIndexPrefix}{blockIndex}").ToNumberValue();

        if (!forSource.IsReversed)
        {
            return index;
        }

        var length = context.GetValue($"{LoopLengthPrefix}{blockIndex}").ToNumberValue();
        var offset = HasEvaluableOffset(forSource)
            ? context.GetValue($"{LoopOffsetPrefix}{blockIndex}").ToNumberValue()
            : 0;

        return length - 1 - index + offset;
    }

    private static bool HasEvaluableOffset(ForTagSource forSource)
        => forSource.OffsetExpression is { } offset && offset != ContinueOffset;

    private string RenderTemplate(IFluidTemplate template, TemplateContext context, string filePath)
    {
        try
        {
            return template.Render(context);
        }
        catch (InvalidOperationException ex) when (IsRenderStepLimitExceeded(ex))
        {
            throw new InvalidOperationException(
                $"Templating error in '{filePath}': template exceeded the maximum of {limits.MaxRenderSteps} " +
                "rendering steps across the whole file - likely too many '{{ }}' " +
                "expressions per item rather than a large collection (collection size is capped separately " +
                "per loop). Check the file for repeated expressions, or split it into smaller loops.", ex);
        }
    }

    private void ValidateTopLevelLoopRequestCounts(
        string rendered, IReadOnlyList<LoopBlock> blocks, IReadOnlyList<ForTagSource> forSources,
        IReadOnlyList<int> topLevelIndices, IReadOnlySet<int> warnedIndices, string filePath)
    {
        foreach (var rootIndex in topLevelIndices)
        {
            var startMarker = $"{TreeStartMarkerPrefix}{rootIndex}{MarkerSuffix}";
            var endMarker = $"{TreeEndMarkerPrefix}{rootIndex}{MarkerSuffix}";
            var startIndex = rendered.IndexOf(startMarker, StringComparison.Ordinal);
            var endIndex = rendered.IndexOf(endMarker, StringComparison.Ordinal);

            if (startIndex < 0 || endIndex < 0)
            {
                continue;
            }

            var segment = rendered[(startIndex + startMarker.Length)..endIndex];

            var requestCount = RequestSeparatorRegex().Split(segment)
                .Count(fragment => RequestMethodAndUriLineRegex().IsMatch(fragment));

            if (requestCount > limits.MaxExpandedRequests && LoopBlockHierarchy.IsNestingRoot(rootIndex, blocks))
            {
                throw new InvalidOperationException(
                    $"Templating error in '{filePath}': the nested loop tree would expand to {requestCount} " +
                    "requests combined across all nesting levels, exceeding the maximum of " +
                    $"{limits.MaxExpandedRequests} (root loop over '{forSources[rootIndex].CollectionExpression}').");
            }

            if (requestCount == 0 && DeclaresRequests(blocks[rootIndex])
                && !warnedIndices.Any(index => LoopBlockHierarchy.GetAncestorIndices(index, blocks).Contains(rootIndex)))
            {
                logger.LogWarning(
                    "Loop over '{Source}' in '{Path}' generated no requests, although its body declares some " +
                    "(for example because 'limit:'/'offset:' or an '{{% if %}}' excluded every item). Check the " +
                    "loop's collection, options and conditions.",
                    forSources[rootIndex].CollectionExpression,
                    filePath);
            }
        }
    }

    private static bool DeclaresRequests(LoopBlock block) => RequestMethodAndUriLineRegex().IsMatch(block.Body);

    private static string StripTopLevelLoopMarkers(string rendered, IReadOnlyList<int> topLevelIndices)
    {
        foreach (var rootIndex in topLevelIndices)
        {
            rendered = rendered
                .Replace($"{TreeStartMarkerPrefix}{rootIndex}{MarkerSuffix}", string.Empty, StringComparison.Ordinal)
                .Replace($"{TreeEndMarkerPrefix}{rootIndex}{MarkerSuffix}", string.Empty, StringComparison.Ordinal);
        }

        return rendered;
    }

    private static IReadOnlyList<LoopBlock> FindLoopBlocks(ILoopBlockScanner scanner, string content, string filePath)
    {
        try
        {
            return scanner.FindLoopBlocks(content);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException(
                $"Templating error in '{filePath}': {StripTemplatingErrorPrefix(ex.Message)}", ex);
        }
    }

    private static TemplateOptions CreateMemberAccessOptions(string filePath)
        => new()
        {
            MemberAccessStrategy = new UnsafeMemberAccessStrategy(),
            Undefined = name => throw new InvalidOperationException(
                $"Templating error in '{filePath}': '{name}' is undefined.")
        };

    private static LoopSource ResolveSource(
        ICollectionSourceResolver sourceResolver, string sourceExpression, IReadOnlyDictionary<string, object?> model,
        string filePath)
    {
        try
        {
            return sourceResolver.Resolve(sourceExpression, model);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException(
                $"Templating error in '{filePath}': {StripTemplatingErrorPrefix(ex.Message)}", ex);
        }
    }

    private static string StripTemplatingErrorPrefix(string message)
    {
        const string prefix = "Templating error: ";
        return message.StartsWith(prefix, StringComparison.Ordinal) ? message[prefix.Length..] : message;
    }

    private static bool IsRenderStepLimitExceeded(InvalidOperationException ex)
        => ex.Message.Contains(FluidMessageFormats.RenderStepLimitMarker, StringComparison.OrdinalIgnoreCase);

    private static bool IsDynamicSource(
        int blockIndex, IReadOnlyList<LoopBlock> blocks, IReadOnlyList<ForTagSource> forSources)
        => LoopBlockHierarchy.GetAncestorIndices(blockIndex, blocks).Exists(ancestorIndex =>
            FluidExpressionIdentifier.StartsWithIdentifier(
                forSources[blockIndex].CollectionExpression, blocks[ancestorIndex].LoopVariableName));

    [GeneratedRegex(HttpFileParserConstants.HttpRequestSeparatorDirectiveLineRegex)]
    private static partial Regex RequestSeparatorRegex();

    [GeneratedRegex(HttpFileParserConstants.RequestMethodAndUriLinePattern, RegexOptions.IgnoreCase)]
    private static partial Regex RequestMethodAndUriLineRegex();
}
