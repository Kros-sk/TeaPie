using Microsoft.Extensions.Logging;
using TeaPie.Logging;
using TeaPie.Pipelines;
using TeaPie.StructureExploration;
using TeaPie.StructureExploration.Paths;
using Timer = TeaPie.Logging.Timer;

namespace TeaPie.Scripts;

internal sealed class PreProcessScriptStep(
    IPipeline pipeline,
    IScriptExecutionContextAccessor scriptExecutionContextAccessor,
    IScriptPreProcessor scriptPreProcessor,
    IExternalFileRegistry externalFileRegistry) : IPipelineStep
{
    private readonly IScriptExecutionContextAccessor _scriptContextAccessor = scriptExecutionContextAccessor;
    private readonly IPipeline _pipeline = pipeline;
    private readonly IScriptPreProcessor _scriptPreProcessor = scriptPreProcessor;
    private readonly IExternalFileRegistry _externalFileRegistry = externalFileRegistry;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(out var scriptExecutionContext);

        var referencedScriptsPaths = await ProcessScript(context, scriptExecutionContext);

        HandleReferencedScripts(context, referencedScriptsPaths);
    }

    private async Task<List<ScriptReference>> ProcessScript(
        ApplicationContext context, ScriptExecutionContext scriptExecutionContext)
    {
        LogPreprocessStart(context, scriptExecutionContext);

        var referencedScriptsPaths = new List<ScriptReference>();

        await Timer.Execute(
            async () => await _scriptPreProcessor.ProcessScript(scriptExecutionContext, referencedScriptsPaths),
            elapsedTime => LogEndOfPreprocess(context, scriptExecutionContext, elapsedTime));

        return referencedScriptsPaths;
    }

    private void HandleReferencedScripts(ApplicationContext context, List<ScriptReference> referencedScriptsPaths)
    {
        foreach (var scriptReference in referencedScriptsPaths)
        {
            if (!context.UserDefinedScripts.ContainsKey(scriptReference.RealPath))
            {
                InsertStepsForScript(context, scriptReference);
            }
        }
    }

    private void InsertStepsForScript(ApplicationContext context, ScriptReference scriptReference)
    {
        PrepareSteps(context, scriptReference, out var relativePath, out var script, out var steps);

        _pipeline.InsertSteps(this, steps);

        context.Logger.LogDebug(
            "For the referenced script '{RefScriptPath}', {Count} steps of pre-process were scheduled in the pipeline.",
            relativePath,
            steps.Length);

        context.RegisterUserDefinedScript(scriptReference.RealPath, script);
    }

    private void PrepareSteps(
        ApplicationContext context,
        ScriptReference scriptReference,
        out string relativePath,
        out Script script,
        out IPipelineStep[] steps)
    {
        relativePath = scriptReference.RealPath.TrimRootPath(context.Path, true);

        script = GetScript(context, scriptReference, relativePath);

        var scriptContext = new ScriptExecutionContext(script);
        steps = ScriptStepsFactory.CreateStepsForScriptPreProcess(context.ServiceProvider, scriptContext);
    }

    private Script GetScript(ApplicationContext context, ScriptReference scriptReference, string relativePath)
    {
        if (scriptReference.IsExternal)
        {
            var file = _externalFileRegistry.Get(scriptReference.RealPath);
            return new(file);
        }
        else if (context.CollectionStructure.TryGetFolder(
            Path.GetDirectoryName(scriptReference.RealPath) ?? string.Empty, out var folder))
        {
            return new Script(
                new InternalFile(scriptReference.RealPath, relativePath, folder));
        }

        throw new InvalidOperationException($"Unable to find script at path '{scriptReference.RealPath}'.");
    }

    private static void LogPreprocessStart(ApplicationContext context, ScriptExecutionContext scriptExecutionContext)
        => context.Logger.LogTrace("Pre-process of the script at path '{ScriptPath}' started.",
            scriptExecutionContext.Script.File.GetDisplayPath());

    private static void LogEndOfPreprocess(ApplicationContext context, ScriptExecutionContext scriptExecutionContext, long elapsedTime)
        => context.Logger.LogTrace("Pre-process of the script at path '{ScriptPath}' finished in {Time}.",
            scriptExecutionContext.Script.File.GetDisplayPath(),
            elapsedTime.ToHumanReadableTime());

    private void ValidateContext(out ScriptExecutionContext scriptExecutionContext)
    {
        const string activityName = "pre-process script";
        ExecutionContextValidator.Validate(_scriptContextAccessor, out scriptExecutionContext, activityName);
        ExecutionContextValidator.ValidateParameter(
            scriptExecutionContext.RawContent, out _, activityName, "its content");
    }
}
