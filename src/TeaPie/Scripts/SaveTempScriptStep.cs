using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.Scripts;

internal sealed class SaveTempScriptStep(IScriptExecutionContextAccessor accessor, TemporaryPathResolver temporaryPathResolver)
    : IPipelineStep
{
    private readonly IScriptExecutionContextAccessor _scriptContextAccessor = accessor;
    private readonly TemporaryPathResolver _temporaryPathResolver = temporaryPathResolver;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(out var scriptExecutionContext, out var content);

        var temporaryPath = await SaveTemporaryScript(scriptExecutionContext, content, cancellationToken);

        context.Logger.LogTrace(
            "Pre-processed script from path '{ScriptPath}' was saved to temporary folder - to path '{TempPath}'",
            scriptExecutionContext.Script.File.GetDisplayPath(),
            temporaryPath);
    }

    private async Task<string> SaveTemporaryScript(
        ScriptExecutionContext scriptExecution,
        string content,
        CancellationToken cancellationToken)
    {
        var temporaryPath = _temporaryPathResolver.ResolvePath(scriptExecution.Script.File.Path, string.Empty);

        Directory.CreateDirectory(Path.GetDirectoryName(temporaryPath)!);
        await File.WriteAllTextAsync(temporaryPath, content, cancellationToken);

        return temporaryPath;
    }

    private void ValidateContext(out ScriptExecutionContext scriptExecutionContext, out string content)
    {
        const string activityName = "save temporary script";
        ExecutionContextValidator.Validate(_scriptContextAccessor, out scriptExecutionContext, activityName);
        ExecutionContextValidator.ValidateParameter(
            scriptExecutionContext.ProcessedContent, out content, activityName, "its processed content");
    }
}
