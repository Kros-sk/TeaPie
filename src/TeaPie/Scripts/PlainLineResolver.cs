namespace TeaPie.Scripts;

internal class PlainLineResolver : IScriptLineResolver
{
    public bool CanResolve(string line) => true;

    public Task<string> ResolveLine(string line, ScriptPreProcessContext context)
        => Task.FromResult(line);
}
