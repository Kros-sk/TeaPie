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

    public static CompileScriptStep Create(ScriptExecutionContext scriptExecution, IScriptCompiler scriptCompiler)
        => new(scriptExecution, scriptCompiler);

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        if (_script.ProcessedContent is null)
        {
            throw new InvalidOperationException("Script can not be compiled, when pre-processed content is null.");
        }

        var (script, compilation) = _compiler.CompileScript(_script.ProcessedContent);
        _script.ScriptObject = script;
        _script.Compilation = compilation;

        await Task.CompletedTask;
    }
}
