using System.Text;
using TeaPie.Http.Headers;
using TeaPie.Http.Retrying;
using TeaPie.Variables;

namespace TeaPie.Http.Parsing;

internal interface IHttpRequestParser
{
    void Parse(RequestExecutionContext requestExecutionContext);
}

internal class HttpRequestParser(
    IHttpRequestHeadersProvider headersProvider,
    IVariablesResolver variablesResolver,
    IHeadersHandler headersResolver,
    IRetryingHandler retryingHandler)
    : IHttpRequestParser
{
    private readonly IHttpRequestHeadersProvider _headersProvider = headersProvider;
    private readonly IVariablesResolver _variablesResolver = variablesResolver;
    private readonly IHeadersHandler _headersResolver = headersResolver;
    private readonly IRetryingHandler _retryingHandler = retryingHandler;

    private readonly IEnumerable<ILineParser> _lineParsers =
    [
        new CommentLineParser(),
        new RetryDirectivesLineParser(),
        new EmptyLineParser(),
        new MethodAndUriParser(),
        new HeaderParser(),
        new BodyParser()
    ];

    public void Parse(RequestExecutionContext requestExecutionContext)
    {
        var parsingContext = new HttpParsingContext(_headersProvider.GetDefaultHeaders());

        if (requestExecutionContext.RawContent is null)
        {
            throw new InvalidOperationException("Unable to parse file, which content is null.");
        }

        foreach (var line in requestExecutionContext.RawContent.Split(Environment.NewLine))
        {
            var resolvedLine = _variablesResolver.ResolveVariablesInLine(line, requestExecutionContext);
            ParseLine(resolvedLine, parsingContext);
        }

        ApplyChanges(requestExecutionContext, parsingContext);
    }

    private void ParseLine(string line, HttpParsingContext context)
    {
        foreach (var parser in _lineParsers)
        {
            if (parser.CanParse(line, context))
            {
                parser.Parse(line, context);
                break;
            }
        }
    }

    private void ApplyChanges(
        RequestExecutionContext requestExecutionContext,
        HttpParsingContext parsingContext)
    {
        var requestMessage = new HttpRequestMessage(parsingContext.Method, parsingContext.RequestUri);

        CreateMessageContent(parsingContext, requestMessage);
        _headersResolver.SetHeaders(parsingContext, requestMessage);

        requestExecutionContext.Request = requestMessage;

        if (!parsingContext.RequestName.Equals(string.Empty))
        {
            requestExecutionContext.Name = parsingContext.RequestName;
        }

        requestExecutionContext.ResiliencePipeline =
            _retryingHandler.GetResiliencePipeline(
                parsingContext.RetryStrategyName,
                parsingContext.ExplicitRetryStrategy,
                parsingContext.RetryUntilStatusCodes);
    }

    private static void CreateMessageContent(HttpParsingContext context, HttpRequestMessage requestMessage)
    {
        var bodyContent = context.BodyBuilder.ToString().Trim();
        if (!string.IsNullOrEmpty(bodyContent))
        {
            requestMessage.Content = new StringContent(bodyContent, Encoding.UTF8);
        }
    }
}
