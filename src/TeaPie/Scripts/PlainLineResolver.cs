namespace TeaPie.Scripts;

internal class PlainLineResolver : IScriptLineResolver
{
    public Task<string> ResolveLine(string line, ScriptPreProcessContext context)
        => Task.FromResult(line);

    public bool CanResolve(string line) => true;
}
