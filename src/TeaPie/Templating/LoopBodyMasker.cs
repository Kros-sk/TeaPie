using System.Text.RegularExpressions;

namespace TeaPie.Templating;

internal sealed partial class LoopBodyMasker : ILoopBodyMasker
{
    private readonly record struct AssignOccurrence(string Name, int Position, int? EnclosingBlockIndex);

    public IReadOnlyList<TextEdit> FindMaskEdits(string content, IReadOnlyList<LoopBlock> blocks)
    {
        var rawRanges = RawBlockRegex().Matches(content);
        var tagRanges = TagRegionRegex().Matches(content);
        var assignOccurrences = FindAssignOccurrences(content, blocks, rawRanges);
        var functionTokenMatches = FunctionTokenRegex().Matches(content);
        var plainTokenMatches = TokenRegex().Matches(content);
        var edits = new List<TextEdit>();

        foreach (Match tag in tagRanges)
        {
            var bracePosition = tag.Index - 1;
            if (!IsWithinAnyRange(bracePosition, rawRanges)
                && !IsWithinAnyRange(bracePosition, functionTokenMatches)
                && !IsWithinAnyRange(bracePosition, plainTokenMatches))
            {
                AddPrecedingBracesEdit(content, tag.Index, edits);
            }
        }

        foreach (Match match in FindTokenMatches(functionTokenMatches, plainTokenMatches))
        {
            if (IsWithinAnyRange(match.Index, rawRanges) || IsWithinAnyRange(match.Index, tagRanges))
            {
                continue;
            }

            var inner = StripWhitespaceControl(match.Groups[1].Value).Trim();
            var enclosingBlockIndex = FindEnclosingBlockIndex(match.Index, blocks);

            AddPrecedingBracesEdit(content, match.Index, edits);

            if (!BelongsToScope(inner, match.Index, enclosingBlockIndex, blocks, assignOccurrences))
            {
                edits.Add(new TextEdit(match.Index, match.Length, $"{{% raw %}}{match.Value}{{% endraw %}}"));
            }
        }

        return edits;
    }

    private static string StripWhitespaceControl(string inner)
    {
        var start = inner.StartsWith('-') ? 1 : 0;
        var end = inner.Length > start && inner.EndsWith('-') ? inner.Length - 1 : inner.Length;
        return inner[start..end];
    }

    private static void AddPrecedingBracesEdit(string content, int syntaxIndex, List<TextEdit> edits)
    {
        var start = syntaxIndex;
        while (start > 0 && content[start - 1] == '{')
        {
            start--;
        }

        if (start < syntaxIndex)
        {
            edits.Add(new TextEdit(start, syntaxIndex - start, $"{{% raw %}}{content[start..syntaxIndex]}{{% endraw %}}"));
        }
    }

    private static List<AssignOccurrence> FindAssignOccurrences(
        string content, IReadOnlyList<LoopBlock> blocks, MatchCollection rawRanges)
    {
        var occurrences = new List<AssignOccurrence>();

        foreach (Match match in AssignTargetRegex().Matches(content))
        {
            if (IsWithinAnyRange(match.Index, rawRanges))
            {
                continue;
            }

            occurrences.Add(new AssignOccurrence(
                match.Groups[1].Value, match.Index, FindEnclosingBlockIndex(match.Index, blocks)));
        }

        return occurrences;
    }

    private static int? FindEnclosingBlockIndex(int position, IReadOnlyList<LoopBlock> blocks)
    {
        var enclosing = LoopBlockHierarchy.GetEnclosingBlockIndicesInnermostFirst(position, blocks);
        return enclosing.Count > 0 ? enclosing[0] : null;
    }

    private static bool BelongsToScope(
        string expression,
        int position,
        int? enclosingBlockIndex,
        IReadOnlyList<LoopBlock> blocks,
        List<AssignOccurrence> assignOccurrences)
    {
        if (expression.StartsWith('$'))
        {
            return false;
        }

        if (FluidExpressionIdentifier.ReferencesIdentifier(expression, "forloop"))
        {
            return true;
        }

        if (enclosingBlockIndex is int blockIndex)
        {
            var chain = new List<int> { blockIndex };
            chain.AddRange(LoopBlockHierarchy.GetAncestorIndices(blockIndex, blocks));

            foreach (var ancestorIndex in chain)
            {
                if (FluidExpressionIdentifier.ReferencesIdentifier(expression, blocks[ancestorIndex].LoopVariableName))
                {
                    return true;
                }

                if (assignOccurrences.Exists(occurrence =>
                    occurrence.EnclosingBlockIndex == ancestorIndex
                    && FluidExpressionIdentifier.ReferencesIdentifier(expression, occurrence.Name)))
                {
                    return true;
                }
            }

            var outermostStart = blocks[chain[^1]].StartIndex;
            return assignOccurrences.Exists(occurrence =>
                occurrence.EnclosingBlockIndex is null
                && occurrence.Position < outermostStart
                && FluidExpressionIdentifier.ReferencesIdentifier(expression, occurrence.Name));
        }

        return assignOccurrences.Exists(occurrence =>
            occurrence.EnclosingBlockIndex is null
            && occurrence.Position < position
            && FluidExpressionIdentifier.ReferencesIdentifier(expression, occurrence.Name));
    }

    private static bool IsWithinAnyRange(int index, MatchCollection ranges)
    {
        var low = 0;
        var high = ranges.Count - 1;

        while (low <= high)
        {
            var middle = low + ((high - low) / 2);
            var range = ranges[middle];

            if (index < range.Index)
            {
                high = middle - 1;
            }
            else if (index >= range.Index + range.Length)
            {
                low = middle + 1;
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<Match> FindTokenMatches(
        MatchCollection functionTokenMatches, MatchCollection plainTokenMatches)
    {
        foreach (Match match in functionTokenMatches)
        {
            yield return match;
        }

        foreach (Match match in plainTokenMatches)
        {
            if (!IsWithinAnyRange(match.Index, functionTokenMatches))
            {
                yield return match;
            }
        }
    }

    [GeneratedRegex("\\{\\{((?:\"[^\"]*\"|[^{}\"])*)\\}\\}", RegexOptions.None, TemplatingLimits.RegexTimeoutMilliseconds)]
    private static partial Regex TokenRegex();

    [GeneratedRegex(
        "\\{\\{(\\$(?>\"[^\"]*\"|'[^']*'|\\{\\{(?<D>)|\\}\\}(?<-D>)|[^{}])*(?(D)(?!)))\\}\\}",
        RegexOptions.None, TemplatingLimits.RegexTimeoutMilliseconds)]
    private static partial Regex FunctionTokenRegex();

    [GeneratedRegex(
        @"\{%-?\s*raw\s*-?%\}.*?\{%-?\s*endraw\s*-?%\}", RegexOptions.Singleline, TemplatingLimits.RegexTimeoutMilliseconds)]
    private static partial Regex RawBlockRegex();

    [GeneratedRegex(
        @"\{%-?\s*assign\s+(" + FluidExpressionIdentifier.Pattern + @")\s*=",
        RegexOptions.None, TemplatingLimits.RegexTimeoutMilliseconds)]
    private static partial Regex AssignTargetRegex();

    [GeneratedRegex(@"\{%-?.*?-?%\}", RegexOptions.Singleline, TemplatingLimits.RegexTimeoutMilliseconds)]
    private static partial Regex TagRegionRegex();
}
