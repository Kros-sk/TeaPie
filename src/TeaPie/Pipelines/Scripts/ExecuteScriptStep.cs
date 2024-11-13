using Microsoft.Extensions.Logging;
using TeaPie.Pipelines.Application;
using TeaPie.ScriptHandling;

namespace TeaPie.Pipelines.Scripts;

internal class ExecuteScriptStep(IScriptExecutionContextAccessor scriptExecutionContextAccessor) : IPipelineStep
{
    private readonly IScriptExecutionContextAccessor _scriptContextAccessor = scriptExecutionContextAccessor;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        var scriptExecutionContext = _scriptContextAccessor.ScriptExecutionContext
            ?? throw new NullReferenceException(nameof(_scriptContextAccessor.ScriptExecutionContext));

        var script = scriptExecutionContext.ScriptObject;

        ArgumentNullException.ThrowIfNull(script, nameof(script));

        try
        {
            context.Logger.LogTrace("Execution of the {ScriptType} on path '{RelativePath}' started.",
                GetTypeOfScript(scriptExecutionContext),
                scriptExecutionContext.Script.File.RelativePath);

            await script.RunAsync(
                globals: new Globals() { tp = TeaPie.Instance },
                cancellationToken: cancellationToken);

            context.Logger.LogTrace("Execution of the {ScriptType} on path '{RelativePath}' finished.",
                GetTypeOfScript(scriptExecutionContext),
                scriptExecutionContext.Script.File.RelativePath);
        }
        catch (Exception ex)
        {
            context.Logger.LogError("Error occured during execution of the script on path '{RelativePath}'. Cause: {Cause}.",
                scriptExecutionContext.Script.File.RelativePath, ex.Message);

            throw;
        }
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
}
