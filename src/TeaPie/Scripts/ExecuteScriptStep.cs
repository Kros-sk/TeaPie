using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using TeaPie.Logging;
using TeaPie.Pipelines;
using Timer = TeaPie.Logging.Timer;

namespace TeaPie.Scripts;

internal class ExecuteScriptStep(IScriptExecutionContextAccessor scriptExecutionContextAccessor) : IPipelineStep
{
    private readonly IScriptExecutionContextAccessor _scriptContextAccessor = scriptExecutionContextAccessor;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(out var scriptExecutionContext, out var script);

        await ExecuteScript(context, scriptExecutionContext, script, cancellationToken);
    }

    private static async Task ExecuteScript(
        ApplicationContext context,
        ScriptExecutionContext scriptExecutionContext,
        Script<object> script,
        CancellationToken cancellationToken)
    {
        LogStartOfExecution(context, scriptExecutionContext);
        await ExecuteAndLog(context, scriptExecutionContext, script, cancellationToken);
    }

    private static async Task ExecuteAndLog(
        ApplicationContext context,
        ScriptExecutionContext scriptExecutionContext,
        Script<object> script,
        CancellationToken cancellationToken)
        => await Timer.Execute(
            async () => await script.RunAsync(new Globals() { tp = TeaPie.Instance }, cancellationToken),
            (elapsedTime) => LogEndOfExecution(context, scriptExecutionContext, elapsedTime));

    private static void LogStartOfExecution(ApplicationContext context, ScriptExecutionContext scriptExecutionContext)
        => context.Logger.LogTrace("Execution of the {ScriptType} at path '{RelativePath}' started.",
            GetTypeOfScript(scriptExecutionContext),
            scriptExecutionContext.Script.File.GetDisplayPath());

    private static void LogEndOfExecution(
        ApplicationContext context, ScriptExecutionContext scriptExecutionContext, long elapsedTime)
        => context.Logger.LogTrace("Execution of the {ScriptType} at path '{RelativePath}' finished in {Time}.",
            GetTypeOfScript(scriptExecutionContext),
            scriptExecutionContext.Script.File.GetDisplayPath(),
            elapsedTime.ToHumanReadableTime());

    private static string GetTypeOfScript(ScriptExecutionContext scriptExecutionContext)
    {
        if (string.IsNullOrEmpty(scriptExecutionContext.Script.File.Path))
        {
            return "Unknown script";
        }

        var path = scriptExecutionContext.Script.File.Path;
        return path switch
        {
            var p when p.EndsWith($"{Constants.PreRequestSuffix}{Constants.ScriptFileExtension}") => "Pre-Request script",
            var p when p.EndsWith($"{Constants.PostResponseSuffix}{Constants.ScriptFileExtension}") => "Post-Response script",
            _ => "User-defined script"
        };
    }

    private void ValidateContext(out ScriptExecutionContext scriptExecutionContext, out Script<object> script)
    {
        const string activityName = "execute script";
        ExecutionContextValidator.Validate(_scriptContextAccessor, out scriptExecutionContext, activityName);
        ExecutionContextValidator.ValidateParameter(
            scriptExecutionContext.ScriptObject, out script, activityName, "its script object");
    }
}
