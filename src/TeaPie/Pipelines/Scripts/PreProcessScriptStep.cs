using Microsoft.Extensions.DependencyInjection;
using TeaPie.Helpers;
using TeaPie.Pipelines.Application;
using TeaPie.ScriptHandling;
using TeaPie.StructureExploration;

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

        var referencedScriptsPaths = new List<string>();

        _scriptExecution.ProcessedContent =
            await _scriptPreProcessor.ProcessScript(
                _scriptExecution.Script.File.Path,
                _scriptExecution.RawContent,
                context.Path,
                context.TempFolderPath,
                referencedScriptsPaths);

        HandleReferencedScripts(context, referencedScriptsPaths);
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

                _pipeline.InsertSteps(this,
                    ReadScriptStep.Create(scriptContext),
                    Create(_pipeline, scriptContext, _serviceProvider),
                    SaveTempScriptStep.Create(scriptContext));

                context.UserDefinedScripts.Add(scriptPath, script);
            }
        }
    }
}
