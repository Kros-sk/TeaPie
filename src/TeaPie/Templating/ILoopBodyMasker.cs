namespace TeaPie.Templating;

internal interface ILoopBodyMasker
{
    IReadOnlyList<TextEdit> FindMaskEdits(string content, IReadOnlyList<LoopBlock> blocks);
}
