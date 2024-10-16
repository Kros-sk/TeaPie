using TeaPie.Pipelines.Application;
using TeaPie.ScriptHandling;

namespace TeaPie.Pipelines.Scripts;

internal sealed class ReadScriptStep : IPipelineStep
{
    private readonly ScriptExecutionContext _scriptExecution;

    private ReadScriptStep(ScriptExecutionContext scriptExecution)
    {
        _scriptExecution = scriptExecution;
    }

    public static ReadScriptStep Create(ScriptExecutionContext scriptExecution)
        => new(scriptExecution);

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        _scriptExecution.RawContent = await File.ReadAllTextAsync(_scriptExecution.Script.File.Path, cancellationToken);
    }
}
