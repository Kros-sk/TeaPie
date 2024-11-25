using System.Net.Http.Headers;
using System.Text;
using TeaPie.Requests;
using TeaPie.Variables;

namespace TeaPie.Http;

internal interface IHttpFileParser
{
    HttpRequestMessage Parse(string fileContent, IVariables variables);
}

internal class HttpFileParser : IHttpFileParser
{
    private readonly IHttpRequestHeadersProvider _headersProvider;
    private readonly IEnumerable<ILineParser> _lineParsers;

    public HttpFileParser(IHttpRequestHeadersProvider headersProvider)
    {
        _headersProvider = headersProvider;
        _lineParsers = new List<ILineParser>
        {
            new EmptyLineParser(),
            new MethodAndUriParser(),
            new HeaderParser(),
            new BodyParser()
        };
    }

    public HttpRequestMessage Parse(string fileContent, IVariables variables)
    {
        var context = new HttpParsingContext(_headersProvider.GetDefaultHeaders());

        foreach (var line in fileContent.Split(Environment.NewLine))
        {
            foreach (var parser in _lineParsers)
            {
                if (parser.CanParse(line, context.IsBody))
                {
                    parser.Parse(line, context);
                    break;
                }
            }
        }

        return CreateHttpRequestMessage(context);
    }

    private static HttpRequestMessage CreateHttpRequestMessage(HttpParsingContext context)
    {
        var requestMessage = new HttpRequestMessage(context.Method, context.RequestUri);

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

        foreach (var header in context.Headers)
        {
            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return requestMessage;
    }
}
