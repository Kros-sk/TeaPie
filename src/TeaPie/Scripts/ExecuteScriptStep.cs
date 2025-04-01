using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;

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
        context.Logger.LogTrace("Execution of the {ScriptType} on path '{RelativePath}' started.",
            GetTypeOfScript(scriptExecutionContext),
            scriptExecutionContext.Script.File.GetDisplayPath());

        await script.RunAsync(
            globals: new Globals() { tp = TeaPie.Instance },
            cancellationToken: cancellationToken);

        context.Logger.LogTrace("Execution of the {ScriptType} on path '{RelativePath}' finished.",
            GetTypeOfScript(scriptExecutionContext),
            scriptExecutionContext.Script.File.GetDisplayPath());
    }

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
