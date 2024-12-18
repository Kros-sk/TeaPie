using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;

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

        try
        {
            context.Logger.LogTrace("Compilation of the script on path '{ScriptPath}' started.",
                scriptExecutionContext.Script.File.RelativePath);

            scriptExecutionContext.ScriptObject = _compiler.CompileScript(content);

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

    private void ValidateContext(out ScriptExecutionContext scriptExecutionContext, out string content)
    {
        scriptExecutionContext = _scriptContextAccessor.ScriptExecutionContext
            ?? throw new InvalidOperationException("Unable to compile script if script's execution context is null.");

        content = scriptExecutionContext.ProcessedContent
            ?? throw new InvalidOperationException("Unable to compile script if its pre-processed content is null.");
    }
}
