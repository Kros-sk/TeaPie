using TeaPie.Pipelines.Application;
using TeaPie.ScriptHandling;

namespace TeaPie.Pipelines.Scripts;
internal class ReadScriptStep(ScriptExecution scriptExecution) : IPipelineStep
{
    private readonly ScriptExecution _scriptExecution = scriptExecution;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        _scriptExecution.RawContent = await File.ReadAllTextAsync(_scriptExecution.Script.File.Path, cancellationToken);
    }
}
