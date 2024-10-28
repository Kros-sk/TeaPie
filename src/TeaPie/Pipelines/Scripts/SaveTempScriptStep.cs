using Microsoft.Extensions.Logging;
using TeaPie.Pipelines.Application;

namespace TeaPie.Pipelines.Scripts;

internal sealed class SaveTempScriptStep(IScriptExecutionContextAccessor accessor) : IPipelineStep
{
    private readonly IScriptExecutionContextAccessor _accessor = accessor;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        var scriptExecution = _accessor.ScriptExecutionContext ?? throw new ArgumentNullException("Script execution context");

        if (scriptExecution.ProcessedContent is null)
        {
            throw new InvalidOperationException(
                "Processed content of the script can not be null when storing to temporary script file.");
        }

        var tmpPath = Path.Combine(context.TempFolderPath, scriptExecution.Script.File.RelativePath);

        var parent = Directory.GetParent(tmpPath);
        ArgumentNullException.ThrowIfNull(parent);

        if (!Directory.Exists(parent.FullName))
        {
            Directory.CreateDirectory(parent.FullName);
        }

        await File.WriteAllTextAsync(tmpPath, scriptExecution.ProcessedContent, cancellationToken);

        scriptExecution.TemporaryPath = tmpPath;

        context.Logger.LogTrace("Pre-processed script from path '{ScriptPath}' was saved to temporary folder," +
            " on path '{TempPath}'",
            scriptExecution.Script.File.RelativePath,
            tmpPath);
    }
}
