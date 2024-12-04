using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;

namespace TeaPie.Http;

internal class ParseHttpRequestStep(IRequestExecutionContextAccessor contextAccessor, IHttpFileParser parser) : IPipelineStep
{
    private readonly IRequestExecutionContextAccessor _requestExecutionContextAccessor = contextAccessor;
    private readonly IHttpFileParser _parser = parser;

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

        context.Logger.LogTrace("Parsing of the request on path '{Path}' finished successfully.",
            requestExecutionContext.RequestFile.RelativePath);

        await Task.CompletedTask;
    }
}
