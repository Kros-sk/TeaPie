using System.Text;
using TeaPie.Http.Auth;
using TeaPie.Http.Headers;
using TeaPie.Http.Retrying;
using TeaPie.Testing;
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
    IResiliencePipelineProvider resiliencePipelineProvider,
    IAuthProviderRegistry authProviderRegistry,
    ITestScheduler testScheduler)
    : IHttpRequestParser
{
    private readonly IHttpRequestHeadersProvider _headersProvider = headersProvider;
    private readonly IVariablesResolver _variablesResolver = variablesResolver;
    private readonly IHeadersHandler _headersResolver = headersResolver;
    private readonly IResiliencePipelineProvider _resiliencePipelineProvider = resiliencePipelineProvider;
    private readonly IAuthProviderRegistry _authProviderRegistry = authProviderRegistry;
    private readonly ITestScheduler _testScheduler = testScheduler;

    private readonly IEnumerable<ILineParser> _lineParsers =
    [
        new CommentLineParser(),
        new DirectivesLineParser(),
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

        UpdateState(requestExecutionContext, parsingContext);
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

    private void UpdateState(RequestExecutionContext requestExecutionContext, HttpParsingContext parsingContext)
    {
        UpdateRequestExecutionContext(requestExecutionContext, parsingContext);
        UpdateScheduledTests(requestExecutionContext, parsingContext);
    }

    private void UpdateRequestExecutionContext(
        RequestExecutionContext requestExecutionContext, HttpParsingContext parsingContext)
    {
        var requestMessage = new HttpRequestMessage(parsingContext.Method, parsingContext.RequestUri);

        UpdateRequestMessage(requestExecutionContext, parsingContext, requestMessage);
        UpdateAuthentication(requestExecutionContext, parsingContext);
        UpdateResiliencePipeline(requestExecutionContext, parsingContext);
    }

    private void UpdateScheduledTests(RequestExecutionContext requestExecutionContext, HttpParsingContext parsingContext)
    {
        if (parsingContext.ScheduledTests.Count > 0)
        {
            foreach (var testDescription in parsingContext.ScheduledTests)
            {
                testDescription.SetRequestExecutionContext(requestExecutionContext);
                _testScheduler.Schedule(PredefinedTestFactory.Create(testDescription));
            }
        }
    }

    private void UpdateRequestMessage(
        RequestExecutionContext requestExecutionContext, HttpParsingContext parsingContext, HttpRequestMessage requestMessage)
    {
        CreateMessageContent(parsingContext, requestMessage);
        _headersResolver.SetHeaders(parsingContext, requestMessage);

        requestExecutionContext.Request = requestMessage;

        if (!parsingContext.RequestName.Equals(string.Empty))
        {
            requestExecutionContext.Name = parsingContext.RequestName;
        }
    }

    private void UpdateAuthentication(RequestExecutionContext requestExecutionContext, HttpParsingContext parsingContext)
    {
        if (!parsingContext.AuthProviderName.Equals(string.Empty))
        {
            requestExecutionContext.AuthProvider = _authProviderRegistry.Get(parsingContext.AuthProviderName);
        }
    }

    private void UpdateResiliencePipeline(RequestExecutionContext requestExecutionContext, HttpParsingContext parsingContext)
        => requestExecutionContext.ResiliencePipeline =
            _resiliencePipelineProvider.GetResiliencePipeline(
                parsingContext.RetryStrategyName,
                parsingContext.ExplicitRetryStrategy,
                parsingContext.RetryUntilStatusCodes);

    private static void CreateMessageContent(HttpParsingContext context, HttpRequestMessage requestMessage)
    {
        var bodyContent = context.BodyBuilder.ToString().Trim();
        if (!string.IsNullOrEmpty(bodyContent))
        {
            requestMessage.Content = new StringContent(bodyContent, Encoding.UTF8);
        }
    }
}
