using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeaPie.Pipelines.Application;
using TeaPie.ScriptHandling;

namespace TeaPie.Pipelines.Scripts;

internal sealed class CompileScriptStep : IPipelineStep
{
    private readonly ScriptExecutionContext _script;
    private readonly IScriptCompiler _compiler;

    private CompileScriptStep(ScriptExecutionContext scriptExecution, IScriptCompiler scriptCompiler)
    {
        _script = scriptExecution;
        _compiler = scriptCompiler;
    }

    public static CompileScriptStep Create(ScriptExecutionContext scriptExecution, IServiceProvider serviceProvider)
        => new(scriptExecution, serviceProvider.GetRequiredService<IScriptCompiler>());

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        if (_script.ProcessedContent is null)
        {
            throw new InvalidOperationException("Script can not be compiled, when pre-processed content is null.");
        }

        context.Logger.LogTrace("Compilation of the script on path '{ScriptPath}' started.", _script.Script.File.RelativePath);

        var (script, compilation) = _compiler.CompileScript(_script.ProcessedContent);
        _script.ScriptObject = script;
        _script.Compilation = compilation;

        context.Logger.LogTrace("Compilation of the script on path '{ScriptPath}' finished.", _script.Script.File.RelativePath);

        await Task.CompletedTask;
    }
}
