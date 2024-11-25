using Microsoft.Extensions.Logging;
using TeaPie.Http;
using TeaPie.Pipelines.Application;

namespace TeaPie.Pipelines.Requests;

internal class ParseRequestFileStep(IRequestExecutionContextAccessor contextAccessor, IHttpFileParser parser) : IPipelineStep
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

        requestExecutionContext.Request = _parser.Parse(requestExecutionContext.RawContent, context.Variables);

        context.Logger.LogTrace("Parsing of the request on path '{Path}' finished successfully.",
            requestExecutionContext.RequestFile.RelativePath);

        await Task.CompletedTask;
    }
}
