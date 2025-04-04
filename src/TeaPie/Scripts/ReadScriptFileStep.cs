using Microsoft.Extensions.Logging;
using TeaPie.Logging;
using TeaPie.Pipelines;
using Timer = TeaPie.Logging.Timer;

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
        scriptExecutionContext.RawContent = await Timer.Execute(
            async () => await File.ReadAllTextAsync(scriptExecutionContext.Script.File.Path, cancellationToken),
            elapsedTime => LogEndOfReading(context, scriptExecutionContext, elapsedTime));
    }

    private static void LogEndOfReading(
        ApplicationContext context, ScriptExecutionContext scriptExecutionContext, long elapsedTime)
        => context.Logger.LogTrace("Content of the script file at path '{ScriptPath}' was read in {Time}",
            scriptExecutionContext.Script.File.GetDisplayPath(),
            elapsedTime.ToHumanReadableTime());

    private void ValidateContext(out ScriptExecutionContext scriptExecutionContext)
        => ExecutionContextValidator.Validate(_scriptContextAccessor, out scriptExecutionContext, "read script");
}
