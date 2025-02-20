using Microsoft.Extensions.Logging;
using TeaPie.Application;
using TeaPie.Pipelines;

namespace TeaPie.Http;

internal class ParseHttpRequestStep(IRequestExecutionContextAccessor contextAccessor, IHttpRequestParser parser) : IPipelineStep
{
    private readonly IRequestExecutionContextAccessor _requestExecutionContextAccessor = contextAccessor;
    private readonly IHttpRequestParser _parser = parser;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(out var requestExecutionContext);

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

    private void ValidateContext(out RequestExecutionContext requestExecutionContext)
    {
        const string activityName = "parse HTTP request";
        ExecutionContextValidator.Validate(_requestExecutionContextAccessor, out requestExecutionContext, activityName);
        ExecutionContextValidator.ValidateParameter(
            requestExecutionContext.RawContent, out _, activityName, "its content");
    }
}
