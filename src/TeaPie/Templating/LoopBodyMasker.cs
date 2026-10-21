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
        var edits = new List<TextEdit>();

        foreach (Match match in FindTokenMatches(content))
        {
            if (IsWithinAnyRange(match.Index, rawRanges) || IsWithinAnyRange(match.Index, tagRanges))
            {
                continue;
            }

            var inner = match.Groups[1].Value.Trim();
            var enclosingBlockIndex = FindEnclosingBlockIndex(match.Index, blocks);

            if (!BelongsToScope(inner, match.Index, enclosingBlockIndex, blocks, assignOccurrences))
            {
                edits.Add(new TextEdit(match.Index, match.Length, $"{{% raw %}}{match.Value}{{% endraw %}}"));
            }
        }

        return edits;
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
        if (FluidExpressionIdentifier.StartsWithIdentifier(expression, "forloop"))
        {
            return true;
        }

        if (enclosingBlockIndex is int blockIndex)
        {
            var chain = new List<int> { blockIndex };
            chain.AddRange(LoopBlockHierarchy.GetAncestorIndices(blockIndex, blocks));

            foreach (var ancestorIndex in chain)
            {
                if (FluidExpressionIdentifier.StartsWithIdentifier(expression, blocks[ancestorIndex].LoopVariableName))
                {
                    return true;
                }

                if (assignOccurrences.Exists(occurrence =>
                    occurrence.EnclosingBlockIndex == ancestorIndex
                    && FluidExpressionIdentifier.StartsWithIdentifier(expression, occurrence.Name)))
                {
                    return true;
                }
            }

            var outermostStart = blocks[chain[^1]].StartIndex;
            return assignOccurrences.Exists(occurrence =>
                occurrence.EnclosingBlockIndex is null
                && occurrence.Position < outermostStart
                && FluidExpressionIdentifier.StartsWithIdentifier(expression, occurrence.Name));
        }

        return assignOccurrences.Exists(occurrence =>
            occurrence.EnclosingBlockIndex is null
            && occurrence.Position < position
            && FluidExpressionIdentifier.StartsWithIdentifier(expression, occurrence.Name));
    }

    private static bool IsWithinAnyRange(int index, MatchCollection ranges)
    {
        foreach (Match range in ranges)
        {
            if (index >= range.Index && index < range.Index + range.Length)
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<Match> FindTokenMatches(string content)
    {
        var functionTokenMatches = FunctionTokenRegex().Matches(content);

        foreach (Match match in functionTokenMatches)
        {
            yield return match;
        }

        foreach (Match match in TokenRegex().Matches(content))
        {
            if (!IsWithinAnyRange(match.Index, functionTokenMatches))
            {
                yield return match;
            }
        }
    }

    [GeneratedRegex("\\{\\{((?:\"[^\"]*\"|[^{}\"])*)\\}\\}")]
    private static partial Regex TokenRegex();

    [GeneratedRegex(
        "\\{\\{(\\$(?:\"[^\"]*\"|'[^']*'|\\{\\{(?<D>)|\\}\\}(?<-D>)|[^{}])*(?(D)(?!)))\\}\\}")]
    private static partial Regex FunctionTokenRegex();

    [GeneratedRegex(@"\{%-?\s*raw\s*-?%\}.*?\{%-?\s*endraw\s*-?%\}", RegexOptions.Singleline)]
    private static partial Regex RawBlockRegex();

    [GeneratedRegex(@"\{%-?\s*assign\s+([A-Za-z_][A-Za-z0-9_]*)\s*=")]
    private static partial Regex AssignTargetRegex();

    [GeneratedRegex(@"\{%-?.*?-?%\}", RegexOptions.Singleline)]
    private static partial Regex TagRegionRegex();
}
