using TeaPie.Pipelines;

namespace TeaPie.Http;

internal class DisposeRequestStep(IRequestExecutionContextAccessor contextAccessor) : IPipelineStep
{
    private readonly IRequestExecutionContextAccessor _requestExecutionContextAccessor = contextAccessor;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(out var requestExecutionContext);
        requestExecutionContext.Dispose();
        await Task.CompletedTask;
    }

    private void ValidateContext(out RequestExecutionContext requestExecutionContext)
    {
        const string activityName = "dispose HTTP request";
        ExecutionContextValidator.Validate(_requestExecutionContextAccessor, out requestExecutionContext, activityName);
    }
}
