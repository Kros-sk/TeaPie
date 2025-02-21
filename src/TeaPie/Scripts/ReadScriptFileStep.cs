using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;

namespace TeaPie.Scripts;

internal sealed class ReadScriptFileStep(IScriptExecutionContextAccessor scriptExecutionContextAccessor) : IPipelineStep
{
    private readonly IScriptExecutionContextAccessor _scriptContextAccessor = scriptExecutionContextAccessor;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(out var scriptExecutionContext);

        await ReadScriptFile(context, scriptExecutionContext, cancellationToken);
    }

    private static async Task ReadScriptFile(
        ApplicationContext context,
        ScriptExecutionContext scriptExecutionContext,
        CancellationToken cancellationToken)
    {
        scriptExecutionContext.RawContent =
            await File.ReadAllTextAsync(scriptExecutionContext.Script.File.Path, cancellationToken);

        context.Logger.LogTrace("Content of the script file on path '{ScriptPath}' was read.",
            scriptExecutionContext.Script.File.RelativePath);
    }

    private void ValidateContext(out ScriptExecutionContext scriptExecutionContext)
        => ExecutionContextValidator.Validate(_scriptContextAccessor, out scriptExecutionContext, "read script");
}
