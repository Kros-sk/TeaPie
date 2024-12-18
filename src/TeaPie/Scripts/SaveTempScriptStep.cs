using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;

namespace TeaPie.Scripts;

internal sealed class SaveTempScriptStep(IScriptExecutionContextAccessor accessor) : IPipelineStep
{
    private readonly IScriptExecutionContextAccessor _accessor = accessor;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(out var scriptExecution, out var content);

        var temporaryPath = await SaveTemporaryScript(context, scriptExecution, content, cancellationToken);

        context.Logger.LogTrace(
            "Pre-processed script from path '{ScriptPath}' was saved to temporary folder, on path '{TempPath}'",
            scriptExecution.Script.File.RelativePath,
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

    private void ValidateContext(out ScriptExecutionContext scriptExecution, out string content)
    {
        scriptExecution = _accessor.ScriptExecutionContext
            ?? throw new InvalidOperationException("Unable to save temporary script if script's execution context is null.");

        content = scriptExecution.ProcessedContent
            ?? throw new InvalidOperationException("Unable to save temporary script if processed content is null.");
    }
}
