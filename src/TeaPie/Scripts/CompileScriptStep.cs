using Microsoft.Extensions.Logging;
using TeaPie.Logging;
using TeaPie.Pipelines;
using Timer = TeaPie.Logging.Timer;

namespace TeaPie.Scripts;

internal sealed class CompileScriptStep(
    IScriptExecutionContextAccessor scriptExecutionContextAccessor,
    IScriptCompiler scriptCompiler)
    : IPipelineStep
{
    private readonly IScriptExecutionContextAccessor _scriptContextAccessor = scriptExecutionContextAccessor;
    private readonly IScriptCompiler _compiler = scriptCompiler;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(out var scriptExecutionContext, out var content);

        CompileScript(context, scriptExecutionContext, content);

        await Task.CompletedTask;
    }

    private void CompileScript(ApplicationContext context, ScriptExecutionContext scriptExecutionContext, string content)
    {
        LogCompilationStart(context, scriptExecutionContext);

        scriptExecutionContext.ScriptObject = Timer.Execute(
            () => _compiler.CompileScript(content, scriptExecutionContext.Script.File.GetDisplayPath()),
            elapsedTime => LogEndOfCompilation(context, scriptExecutionContext, elapsedTime));
    }

    private static void LogCompilationStart(ApplicationContext context, ScriptExecutionContext scriptExecutionContext)
        => context.Logger.LogTrace("Compilation of the script at path '{ScriptPath}' started.",
            scriptExecutionContext.Script.File.GetDisplayPath());

    private static void LogEndOfCompilation(
        ApplicationContext context, ScriptExecutionContext scriptExecutionContext, long elapsedTime)
        => context.Logger.LogTrace("Compilation of the script at path '{ScriptPath}' finished successfully in {Time}.",
            scriptExecutionContext.Script.File.GetDisplayPath(),
            elapsedTime.ToHumanReadableTime());

    private void ValidateContext(out ScriptExecutionContext scriptExecutionContext, out string content)
    {
        const string activityName = "compile script";
        ExecutionContextValidator.Validate(_scriptContextAccessor, out scriptExecutionContext, activityName);
        ExecutionContextValidator.ValidateParameter(
            scriptExecutionContext.ProcessedContent, out content, activityName, "its processed content");
    }
}
