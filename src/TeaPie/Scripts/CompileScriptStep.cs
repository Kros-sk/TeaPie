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

        CompileScript(context, scriptExecutionContext, content);

        await Task.CompletedTask;
    }

    private void CompileScript(ApplicationContext context, ScriptExecutionContext scriptExecutionContext, string content)
    {
        context.Logger.LogTrace("Compilation of the script on path '{ScriptPath}' started.",
            scriptExecutionContext.Script.File.GetDisplayPath());

        scriptExecutionContext.ScriptObject = _compiler.CompileScript(content, scriptExecutionContext.Script.File.GetDisplayPath());

        context.Logger.LogTrace("Compilation of the script on path '{ScriptPath}' finished successfully.",
            scriptExecutionContext.Script.File.GetDisplayPath());
    }

    private void ValidateContext(out ScriptExecutionContext scriptExecutionContext, out string content)
    {
        const string activityName = "compile script";
        ExecutionContextValidator.Validate(_scriptContextAccessor, out scriptExecutionContext, activityName);
        ExecutionContextValidator.ValidateParameter(
            scriptExecutionContext.ProcessedContent, out content, activityName, "its processed content");
    }
}
