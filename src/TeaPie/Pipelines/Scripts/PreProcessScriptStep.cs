using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeaPie.Helpers;
using TeaPie.Pipelines.Application;
using TeaPie.ScriptHandling;
using TeaPie.StructureExploration.Records;

namespace TeaPie.Pipelines.Scripts;

internal sealed class PreProcessScriptStep : IPipelineStep
{
    private readonly ScriptExecutionContext _scriptExecution;
    private readonly IPipeline _pipeline;
    private readonly IScriptPreProcessor _scriptPreProcessor;
    private readonly IServiceProvider _serviceProvider;

    private PreProcessScriptStep(
        IPipeline pipeline,
        ScriptExecutionContext scriptExecution,
        IScriptPreProcessor scriptPreProcessor,
        IServiceProvider serviceProvider)
    {
        _pipeline = pipeline;
        _scriptExecution = scriptExecution;
        _scriptPreProcessor = scriptPreProcessor;
        _serviceProvider = serviceProvider;
    }

    public static PreProcessScriptStep Create(
        IPipeline pipeline,
        ScriptExecutionContext scriptExecution,
        IServiceProvider serviceProvider)
        => new(pipeline, scriptExecution, serviceProvider.GetRequiredService<IScriptPreProcessor>(), serviceProvider);

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        if (_scriptExecution.RawContent is null)
        {
            throw new InvalidOperationException("Pre-processing of the script can not be done with null content.");
        }

        context.Logger.LogTrace("Pre-process of the script on path '{ScriptPath}' started.",
            _scriptExecution.Script.File.RelativePath);

        var referencedScriptsPaths = new List<string>();

        _scriptExecution.ProcessedContent =
            await _scriptPreProcessor.ProcessScript(
                _scriptExecution.Script.File.Path,
                _scriptExecution.RawContent,
                context.Path,
                context.TempFolderPath,
                referencedScriptsPaths);

        HandleReferencedScripts(context, referencedScriptsPaths);

        context.Logger.LogTrace("Pre-process of the script on path '{ScriptPath}' finished.",
            _scriptExecution.Script.File.RelativePath);
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

                var steps = PrepareSteps(scriptContext);

                _pipeline.InsertSteps(this, steps);

                context.Logger.LogDebug(
                    "For the referenced script '{RefScriptPath}', {Count} steps of pre-process were scheduled in the pipeline.",
                    relativePath,
                    steps.Length);

                context.UserDefinedScripts.Add(scriptPath, script);
            }
        }
    }

    private IPipelineStep[] PrepareSteps(ScriptExecutionContext scriptContext)
        => [ReadFileStep.Create(scriptContext),
            Create(_pipeline, scriptContext, _serviceProvider),
            SaveTempScriptStep.Create(scriptContext)];
}
