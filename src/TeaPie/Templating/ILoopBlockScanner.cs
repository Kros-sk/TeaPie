namespace TeaPie.Templating;

internal interface ILoopBlockScanner
{
    IReadOnlyList<LoopBlock> FindLoopBlocks(string content);
}
