using Microsoft.Extensions.Logging;
using TeaPie.Application;
using TeaPie.Pipelines;

namespace TeaPie.Scripts;

internal sealed class SaveTempScriptStep(IScriptExecutionContextAccessor accessor) : IPipelineStep
{
    private readonly IScriptExecutionContextAccessor _scriptContextAccessor = accessor;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(out var scriptExecutionContext, out var content);

        var temporaryPath = await SaveTemporaryScript(context, scriptExecutionContext, content, cancellationToken);

        context.Logger.LogTrace(
            "Pre-processed script from path '{ScriptPath}' was saved to temporary folder, on path '{TempPath}'",
            scriptExecutionContext.Script.File.RelativePath,
            temporaryPath);
    }

    private static async Task<string> SaveTemporaryScript(
        ApplicationContext context,
        ScriptExecutionContext scriptExecution,
        string content,
        CancellationToken cancellationToken)
    {
        var temporaryPath = Path.Combine(context.TempFolderPath, scriptExecution.Script.File.RelativePath);

        var parent = Directory.GetParent(temporaryPath);
        ArgumentNullException.ThrowIfNull(parent);

        if (!Directory.Exists(parent.FullName))
        {
            Directory.CreateDirectory(parent.FullName);
        }

        await File.WriteAllTextAsync(temporaryPath, content, cancellationToken);

        scriptExecution.TemporaryPath = temporaryPath;
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
