using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using TeaPie.Requests;
using TeaPie.Variables;

namespace TeaPie.Http;

internal interface IHttpFileParser
{
    HttpRequestMessage Parse(string fileContent);
}

internal partial class HttpFileParser(IHttpRequestHeadersProvider headersProvider, IVariables variables) : IHttpFileParser
{
    private readonly IHttpRequestHeadersProvider _headersProvider = headersProvider;
    private readonly IVariables _variables = variables;
    private readonly IEnumerable<ILineParser> _lineParsers =
        [
            new CommentLineParser(),
            new RequestSeparatorParser(),
            new EmptyLineParser(),
            new MethodAndUriParser(),
            new HeaderParser(),
            new BodyParser()
        ];

    public HttpRequestMessage Parse(string fileContent)
    {
        var context = new HttpParsingContext(_headersProvider.GetDefaultHeaders());

        foreach (var line in fileContent.Split(Environment.NewLine))
        {
            var resolvedLine = ResolveVariables(line);
            Parse(resolvedLine, context);
        }

        return CreateHttpRequestMessage(context);
    }

    private string ResolveVariables(string line)
        => VariableNotationPatternRegex().Replace(line, match =>
        {
            var variableName = match.Groups[1].Value;
            if (_variables.ContainsVariable(variableName))
            {
                var variableValue = _variables.GetVariable<object>(variableName, default);
                return variableValue?.ToString() ?? "null";
            }

            throw new InvalidOperationException($"Variable '{{{variableName}}}' was not found.");
        });

    private void Parse(string line, HttpParsingContext context)
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

    [GeneratedRegex(HttpFileParserConstants.VariableNotationPattern)]
    private static partial Regex VariableNotationPatternRegex();
}
