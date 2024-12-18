using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;

namespace TeaPie.Http;

internal class ParseHttpRequestStep(IRequestExecutionContextAccessor contextAccessor, IHttpRequestParser parser) : IPipelineStep
{
    private readonly IRequestExecutionContextAccessor _requestExecutionContextAccessor = contextAccessor;
    private readonly IHttpRequestParser _parser = parser;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        var requestExecutionContext = _requestExecutionContextAccessor.RequestExecutionContext
            ?? throw new NullReferenceException("Request's execution context is null.");

        if (requestExecutionContext.RawContent is null)
        {
            throw new InvalidOperationException("Parsing of the request file can not be done with null content.");
        }

        context.Logger.LogTrace("Parsing of the request on path '{Path}' started.",
            requestExecutionContext.RequestFile.RelativePath);

        _parser.Parse(requestExecutionContext);

        requestExecutionContext.TestCaseExecutionContext?.RegisterRequest(
            requestExecutionContext.Request!,
            requestExecutionContext.Name);

        context.Logger.LogTrace("Parsing of the request {RequestName} on path '{Path}' finished successfully.",
            requestExecutionContext.Name.Equals(string.Empty) ? string.Empty : $"'{requestExecutionContext.Name}'",
            requestExecutionContext.RequestFile.RelativePath);

        await Task.CompletedTask;
    }
}
