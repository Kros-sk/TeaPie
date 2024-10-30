using Microsoft.Extensions.Logging;
using TeaPie.Pipelines.Application;
using TeaPie.ScriptHandling;

namespace TeaPie.Pipelines.Scripts;

internal sealed class CompileScriptStep(
    IScriptExecutionContextAccessor scriptExecutionContextAccessor,
    IScriptCompiler scriptCompiler)
    : IPipelineStep
{
    private readonly IScriptExecutionContextAccessor _scriptContextAccessor = scriptExecutionContextAccessor;
    private readonly IScriptCompiler _compiler = scriptCompiler;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        var scriptExecutionContext = _scriptContextAccessor.ScriptExecutionContext
            ?? throw new ArgumentNullException(nameof(_scriptContextAccessor.ScriptExecutionContext));

        if (scriptExecutionContext.ProcessedContent is null)
        {
            throw new InvalidOperationException("Script can not be compiled, when pre-processed content is null.");
        }

        try
        {
            context.Logger.LogTrace("Compilation of the script on path '{ScriptPath}' started.",
                scriptExecutionContext.Script.File.RelativePath);

            scriptExecutionContext.ScriptObject = _compiler.CompileScript(scriptExecutionContext.ProcessedContent);

            context.Logger.LogTrace("Compilation of the script on path '{ScriptPath}' finished successfully.",
                scriptExecutionContext.Script.File.RelativePath);
        }
        catch (Exception ex)
        {
            context.Logger.LogError(message: ex.Message);

            throw;
        }

        await Task.CompletedTask;
    }
}
