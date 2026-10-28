namespace TeaPie.Templating;

internal static class LoopBlockHierarchy
{
    public static List<int> GetEnclosingBlockIndicesInnermostFirst(int position, IReadOnlyList<LoopBlock> blocks)
    {
        var enclosing = new List<int>();

        for (var i = 0; i < blocks.Count; i++)
        {
            if (position >= blocks[i].StartIndex && position < blocks[i].StartIndex + blocks[i].Length)
            {
                enclosing.Add(i);
            }
        }

        enclosing.Sort((a, b) => blocks[a].Length.CompareTo(blocks[b].Length));
        return enclosing;
    }

    public static List<int> GetAncestorIndices(int blockIndex, IReadOnlyList<LoopBlock> blocks)
    {
        var ancestors = GetEnclosingBlockIndicesInnermostFirst(blocks[blockIndex].StartIndex, blocks);
        ancestors.Remove(blockIndex);
        return ancestors;
    }

    public static bool IsTopLevel(int blockIndex, IReadOnlyList<LoopBlock> blocks)
        => GetAncestorIndices(blockIndex, blocks).Count == 0;

    public static bool HasDescendant(int blockIndex, IReadOnlyList<LoopBlock> blocks)
    {
        var block = blocks[blockIndex];

        for (var i = 0; i < blocks.Count; i++)
        {
            if (i != blockIndex
                && blocks[i].StartIndex >= block.StartIndex
                && blocks[i].StartIndex + blocks[i].Length <= block.StartIndex + block.Length)
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsNestingRoot(int blockIndex, IReadOnlyList<LoopBlock> blocks)
        => IsTopLevel(blockIndex, blocks) && HasDescendant(blockIndex, blocks);

    public static bool IsStandaloneBlock(int blockIndex, IReadOnlyList<LoopBlock> blocks)
        => IsTopLevel(blockIndex, blocks) && !HasDescendant(blockIndex, blocks);
}
