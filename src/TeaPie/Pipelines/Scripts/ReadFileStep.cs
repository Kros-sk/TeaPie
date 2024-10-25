using Microsoft.Extensions.Logging;
using TeaPie.Pipelines.Application;
using TeaPie.ScriptHandling;

namespace TeaPie.Pipelines.Scripts;

internal sealed class ReadFileStep : IPipelineStep
{
    private readonly ScriptExecutionContext _scriptExecution;

    private ReadFileStep(ScriptExecutionContext scriptExecution)
    {
        _scriptExecution = scriptExecution;
    }

    public static ReadFileStep Create(ScriptExecutionContext scriptExecution)
        => new(scriptExecution);

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _scriptExecution.RawContent = await File.ReadAllTextAsync(_scriptExecution.Script.File.Path, cancellationToken);
            context.Logger.LogTrace("Content of the file on path '{ScriptPath}' was read.",
                _scriptExecution.Script.File.RelativePath);
        }
        catch (Exception ex)
        {
            context.Logger.LogError("Reading of the script on path '{ScriptPath}' failed, because of '{ErrorMessage}'.",
                _scriptExecution.Script.File.RelativePath,
                ex.Message);

            throw;
        }
    }
}
