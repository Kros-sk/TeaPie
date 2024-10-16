using TeaPie.Pipelines.Application;
using TeaPie.ScriptHandling;

namespace TeaPie.Pipelines.Scripts;

internal sealed class PreProcessScriptStep : IPipelineStep
{
    private readonly ScriptExecutionContext _scriptExecution;
    private readonly IScriptPreProcessor _scriptPreProcessor;

    private PreProcessScriptStep(ScriptExecutionContext scriptExecution, IScriptPreProcessor scriptPreProcessor)
    {
        _scriptExecution = scriptExecution;
        _scriptPreProcessor = scriptPreProcessor;
    }

    public static PreProcessScriptStep Create(ScriptExecutionContext scriptExecution, IScriptPreProcessor scriptPreProcessor)
        => new(scriptExecution, scriptPreProcessor);

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        if (_scriptExecution.RawContent is null)
        {
            throw new InvalidOperationException("Pre-processing of the script can not be done with null content.");
        }

        _scriptExecution.ProcessedContent =
            await _scriptPreProcessor.PrepareScript(_scriptExecution.Script.File.Path, _scriptExecution.RawContent);
    }
}
