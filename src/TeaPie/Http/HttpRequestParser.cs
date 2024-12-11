using System.Net.Http.Headers;
using System.Text;
using TeaPie.Variables;

namespace TeaPie.Http;

internal interface IHttpRequestParser
{
    void Parse(RequestExecutionContext requestExecutionContext);
}

internal class HttpRequestParser(
    IHttpRequestHeadersProvider headersProvider,
    IVariablesResolver variablesResolver, IHeadersResolver headersResolver)
    : IHttpRequestParser
{
    private readonly IHttpRequestHeadersProvider _headersProvider = headersProvider;
    private readonly IVariablesResolver _variablesResolver = variablesResolver;
    private readonly IHeadersResolver _headersResolver = headersResolver;
    private readonly IEnumerable<ILineParser> _lineParsers =
        [
            new CommentLineParser(),
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
        _headersResolver.Resolve(parsingContext, requestMessage);

        requestExecutionContext.Request = requestMessage;

        if (!parsingContext.RequestName.Equals(string.Empty))
        {
            requestExecutionContext.Name = parsingContext.RequestName;
        }
    }

    private static void CreateMessageContent(HttpParsingContext context, HttpRequestMessage requestMessage)
    {
        var bodyContent = context.BodyBuilder.ToString().Trim();
        if (!string.IsNullOrEmpty(bodyContent))
        {
            var content = new StringContent(bodyContent, Encoding.UTF8);

            if (context.Headers.TryGetValues("Content-Type", out var contentType) && contentType?.Count() == 1)
            {
                content.Headers.ContentType = new MediaTypeHeaderValue(contentType.First());
            }

            requestMessage.Content = content;
        }
    }
}
