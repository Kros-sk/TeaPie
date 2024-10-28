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

        context.Logger.LogTrace("Compilation of the script on path '{ScriptPath}' started.",
            scriptExecutionContext.Script.File.RelativePath);

        var (script, compilation) = _compiler.CompileScript(scriptExecutionContext.ProcessedContent);
        scriptExecutionContext.ScriptObject = script;
        scriptExecutionContext.Compilation = compilation;

        context.Logger.LogTrace("Compilation of the script on path '{ScriptPath}' finished.",
            scriptExecutionContext.Script.File.RelativePath);

        await Task.CompletedTask;
    }
}
