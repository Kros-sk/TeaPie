using Microsoft.Extensions.Logging;
using TeaPie.Http.Parsing;
using TeaPie.Logging;
using TeaPie.Pipelines;
using Timer = TeaPie.Logging.Timer;

namespace TeaPie.Http;

internal class ParseHttpRequestStep(IRequestExecutionContextAccessor contextAccessor, IHttpRequestParser parser) : IPipelineStep
{
    private readonly IRequestExecutionContextAccessor _requestExecutionContextAccessor = contextAccessor;
    private readonly IHttpRequestParser _parser = parser;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(out var requestExecutionContext);

        Parse(context, requestExecutionContext);

        requestExecutionContext.TestCaseExecutionContext?.RegisterRequest(
            requestExecutionContext.Request!,
            requestExecutionContext.Name);

        await Task.CompletedTask;
    }

    private void Parse(ApplicationContext context, RequestExecutionContext requestExecutionContext)
    {
        using (context.Logger.BeginTreeScope())
        {
            LogParsingStart(context, requestExecutionContext);

            Timer.Execute(
                () => _parser.Parse(requestExecutionContext),
                elapsedTime => LogEndOfParsing(context, requestExecutionContext, elapsedTime));
        }
    }

    private static void LogParsingStart(ApplicationContext context, RequestExecutionContext requestExecutionContext)
        => context.Logger.LogTrace("Parsing of the request at path '{Path}' started.",
            requestExecutionContext.RequestFile.RelativePath);

    private static void LogEndOfParsing(ApplicationContext context, RequestExecutionContext requestExecutionContext, long elapsedTime)
        => context.Logger.LogTrace("Parsing of the request {RequestName} at path '{Path}' finished successfully in {Time}.",
            requestExecutionContext.Name.Equals(string.Empty) ? string.Empty : $"'{requestExecutionContext.Name}'",
            requestExecutionContext.RequestFile.RelativePath,
            elapsedTime.ToHumanReadableTime());

    private void ValidateContext(out RequestExecutionContext requestExecutionContext)
    {
        const string activityName = "parse HTTP request";
        ExecutionContextValidator.Validate(_requestExecutionContextAccessor, out requestExecutionContext, activityName);
        ExecutionContextValidator.ValidateParameter(
            requestExecutionContext.RawContent, out _, activityName, "its content");
    }
}
