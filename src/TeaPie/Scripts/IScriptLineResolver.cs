namespace TeaPie.Scripts;

internal interface IScriptLineResolver
{
    Task<string> ResolveLine(string line, ScriptPreProcessContext context);

    bool CanResolve(string line);
}
