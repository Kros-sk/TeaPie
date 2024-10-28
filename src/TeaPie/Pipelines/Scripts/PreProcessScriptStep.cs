using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeaPie.Extensions;
using TeaPie.Pipelines.Application;
using TeaPie.ScriptHandling;
using TeaPie.StructureExploration.Records;

namespace TeaPie.Pipelines.Scripts;

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
        var scriptExecutionContext = _scriptContextAccessor.ScriptExecutionContext
            ?? throw new ArgumentNullException(nameof(_scriptContextAccessor.ScriptExecutionContext));

        if (scriptExecutionContext.RawContent is null)
        {
            throw new InvalidOperationException("Pre-processing of the script can not be done with null content.");
        }

        context.Logger.LogTrace("Pre-process of the script on path '{ScriptPath}' started.",
            scriptExecutionContext.Script.File.RelativePath);

        var referencedScriptsPaths = new List<string>();

        scriptExecutionContext.ProcessedContent =
            await _scriptPreProcessor.ProcessScript(
                scriptExecutionContext.Script.File.Path,
                scriptExecutionContext.RawContent,
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
                var relativePath = scriptPath.TrimRootPath(context.Path, true);

                var folder = context.TestCases.Values
                    .Select(x => x.Request.ParentFolder)
                    .FirstOrDefault(x => x.Path == Directory.GetParent(scriptPath)?.FullName)
                    ?? throw new DirectoryNotFoundException($"One of the directories in the path: {scriptPath} wasn't found");

                var script = new Script(new(scriptPath, relativePath, Path.GetFileName(scriptPath), folder));

                var scriptContext = new ScriptExecutionContext(script);

                var steps = PrepareSteps(context, scriptContext);

                _pipeline.InsertSteps(this, steps);

                context.Logger.LogDebug(
                    "For the referenced script '{RefScriptPath}', {Count} steps of pre-process were scheduled in the pipeline.",
                    relativePath,
                    steps.Length);

                context.UserDefinedScripts.Add(scriptPath, script);
            }
        }
    }

    private static IPipelineStep[] PrepareSteps(ApplicationContext context, ScriptExecutionContext scriptContext)
    {
        using var scope = context.ServiceProvider.CreateScope();
        var provider = scope.ServiceProvider;

        var accessor = provider.GetRequiredService<IScriptExecutionContextAccessor>();
        accessor.ScriptExecutionContext = scriptContext;

        return [provider.GetStep<ReadFileStep>(),
            provider.GetStep<PreProcessScriptStep>(),
            provider.GetStep<SaveTempScriptStep>()];
    }
}
