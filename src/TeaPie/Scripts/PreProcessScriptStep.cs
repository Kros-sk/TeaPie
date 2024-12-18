using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;
using TeaPie.StructureExploration;

namespace TeaPie.Scripts;

internal sealed class PreProcessScriptStep(
    IPipeline pipeline,
    IScriptExecutionContextAccessor scriptExecutionContextAccessor,
    IScriptPreProcessor scriptPreProcessor) : IPipelineStep
{
    private readonly IScriptExecutionContextAccessor _scriptContextAccessor = scriptExecutionContextAccessor;
    private readonly IPipeline _pipeline = pipeline;
    private readonly IScriptPreProcessor _scriptPreProcessor = scriptPreProcessor;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(out var scriptExecutionContext, out var content);

        context.Logger.LogTrace("Pre-process of the script on path '{ScriptPath}' started.",
            scriptExecutionContext.Script.File.RelativePath);

        var referencedScriptsPaths = new List<string>();

        scriptExecutionContext.ProcessedContent =
            await _scriptPreProcessor.ProcessScript(
                scriptExecutionContext.Script.File.Path,
                content,
                context.Path,
                context.TempFolderPath,
                referencedScriptsPaths);

        HandleReferencedScripts(context, referencedScriptsPaths);

        context.Logger.LogTrace("Pre-process of the script on path '{ScriptPath}' finished.",
            scriptExecutionContext.Script.File.RelativePath);
    }

    private void HandleReferencedScripts(ApplicationContext context, List<string> referencedScriptsPaths)
    {
        foreach (var scriptPath in referencedScriptsPaths)
        {
            if (!context.UserDefinedScripts.ContainsKey(scriptPath))
            {
                InsertStepsForScript(context, scriptPath);
            }
        }
    }

    private void InsertStepsForScript(ApplicationContext context, string scriptPath)
    {
        PrepareSteps(context, scriptPath, out var relativePath, out var script, out var steps);

        _pipeline.InsertSteps(this, steps);

        context.Logger.LogDebug(
            "For the referenced script '{RefScriptPath}', {Count} steps of pre-process were scheduled in the pipeline.",
            relativePath,
            steps.Length);

        context.RegisterUserDefinedScript(scriptPath, script);
    }

    private static void PrepareSteps(ApplicationContext context, string scriptPath, out string relativePath, out Script script, out IPipelineStep[] steps)
    {
        relativePath = scriptPath.TrimRootPath(context.Path, true);
        var folder = context.TestCases.Values
            .Select(x => x.RequestsFile.ParentFolder)
            .FirstOrDefault(x => x.Path == Directory.GetParent(scriptPath)?.FullName)
            ?? throw new DirectoryNotFoundException($"One of the directories in the path: {scriptPath} wasn't found");

        script = new Script(new(scriptPath, relativePath, Path.GetFileName(scriptPath), folder));
        var scriptContext = new ScriptExecutionContext(script);

        steps = PrepareSteps(context, scriptContext);
    }

    private static IPipelineStep[] PrepareSteps(ApplicationContext context, ScriptExecutionContext scriptContext)
    {
        using var scope = context.ServiceProvider.CreateScope();
        var provider = scope.ServiceProvider;

        var accessor = provider.GetRequiredService<IScriptExecutionContextAccessor>();
        accessor.ScriptExecutionContext = scriptContext;

        return [provider.GetStep<ReadScriptFileStep>(),
            provider.GetStep<PreProcessScriptStep>(),
            provider.GetStep<SaveTempScriptStep>()];
    }

    private void ValidateContext(out ScriptExecutionContext scriptExecutionContext, out string content)
    {
        scriptExecutionContext = _scriptContextAccessor.ScriptExecutionContext
            ?? throw new InvalidOperationException("Unable to pre-process script if script's execution context is null.");

        content = scriptExecutionContext.RawContent
            ?? throw new InvalidOperationException("Unable to pre-process script if script's content is null.");
    }
}
