using Microsoft.Extensions.Logging;
using TeaPie.Pipelines.Application;

namespace TeaPie.Pipelines.Requests;

internal sealed class ReadRequestFileStep(IRequestExecutionContextAccessor requestExecutionContextAccessor) : IPipelineStep
{
    private readonly IRequestExecutionContextAccessor _requestContextAccessor = requestExecutionContextAccessor;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        var requestExecutionContext = _requestContextAccessor.RequestExecutionContext
            ?? throw new NullReferenceException("Request's execution context is null.");

        try
        {
            requestExecutionContext.RawContent =
                await File.ReadAllTextAsync(requestExecutionContext.RequestFile.Path, cancellationToken);

            context.Logger.LogTrace("Content of the request file on path '{RequestPath}' was read.",
                requestExecutionContext.RequestFile.RelativePath);
        }
        catch (Exception ex)
        {
            context.Logger.LogError("Reading of the request on path '{RequestPath}' failed, because of '{ErrorMessage}'.",
                requestExecutionContext.RequestFile.RelativePath,
                ex.Message);

            throw;
        }
    }
}
