using TeaPie.Pipelines;

namespace TeaPie.Scripts;

internal class DisposeScriptStep(IScriptExecutionContextAccessor scriptExecutionContextAccessor) : IPipelineStep
{
    private readonly IScriptExecutionContextAccessor _scriptContextAccessor = scriptExecutionContextAccessor;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(out var scriptExecutionContext);
        scriptExecutionContext.Dispose();
        await Task.CompletedTask;
    }

    private void ValidateContext(out ScriptExecutionContext scriptExecutionContext)
    {
        const string activityName = "dispose script";
        ExecutionContextValidator.Validate(_scriptContextAccessor, out scriptExecutionContext, activityName);
    }
}
