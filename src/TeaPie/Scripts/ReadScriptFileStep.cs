using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;

namespace TeaPie.Scripts;

internal sealed class ReadScriptFileStep(IScriptExecutionContextAccessor scriptExecutionContextAccessor) : IPipelineStep
{
    private readonly IScriptExecutionContextAccessor _scriptContextAccessor = scriptExecutionContextAccessor;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(out var scriptExecutionContext);

        try
        {
            scriptExecutionContext.RawContent =
                await File.ReadAllTextAsync(scriptExecutionContext.Script.File.Path, cancellationToken);

            context.Logger.LogTrace("Content of the script file on path '{ScriptPath}' was read.",
                scriptExecutionContext.Script.File.RelativePath);
        }
        catch (Exception ex)
        {
            context.Logger.LogError("Reading of the script on path '{ScriptPath}' failed, because of '{ErrorMessage}'.",
                scriptExecutionContext.Script.File.RelativePath,
                ex.Message);

            throw;
        }
    }

    private ScriptExecutionContext ValidateContext(out ScriptExecutionContext scriptExecutionContext)
        => scriptExecutionContext = _scriptContextAccessor.ScriptExecutionContext
            ?? throw new InvalidOperationException("Unable to read script file if script's execution context is null.");
}
