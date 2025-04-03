namespace TeaPie.Scripts;

internal interface IScriptPreProcessor
{
    Task ProcessScript(ScriptExecutionContext script, List<ScriptReference> referencedScripts);
}

internal class ScriptPreProcessor(IScriptLineResolversProvider resolversProvider) : IScriptPreProcessor
{
    private readonly IReadOnlyList<IScriptLineResolver> _scriptLineResolvers = resolversProvider.GetAvailableResolvers();

    public async Task ProcessScript(ScriptExecutionContext scriptContext, List<ScriptReference> referencedScripts)
    {
        CheckContext(scriptContext, out var scriptContent);

        var context = new ScriptPreProcessContext(scriptContext.Script, referencedScripts);

        var resolvedLines = await ResolveLines(scriptContent, context);

        scriptContext.ProcessedContent = GetProcessedContent(resolvedLines);
    }

    private async Task<List<string>> ResolveLines(string scriptContent, ScriptPreProcessContext context)
    {
        var lines = scriptContent.Split(Environment.NewLine, StringSplitOptions.None);
        List<string> resolvedLines = [];

        foreach (var line in lines)
        {
            resolvedLines.Add(await ResolveLine(context, line));
        }

        return resolvedLines;
    }

    private async Task<string> ResolveLine(ScriptPreProcessContext context, string line)
    {
        var resolver = _scriptLineResolvers.FirstOrDefault(r => r.CanResolve(line))
            ?? throw new InvalidOperationException($"Unable to find suitable resolver for script line '{line}'.");

        return await resolver.ResolveLine(line, context);
    }

    private static string? GetProcessedContent(List<string> lines)
    {
        lines = [.. lines.Where(l => !string.IsNullOrEmpty(l))];
        return string.Join(Environment.NewLine, lines);
    }

    private static void CheckContext(ScriptExecutionContext scriptContext, out string scriptContent)
    {
        if (scriptContext.RawContent is null)
        {
            throw new InvalidOperationException("Unable to pre-process script, if content is null.");
        }

        scriptContent = scriptContext.RawContent;
    }
}
