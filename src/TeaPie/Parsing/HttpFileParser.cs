using System.Net.Http.Headers;
using System.Text;

namespace TeaPie.Parsing;

internal static class HttpFileParser
{
    internal static HttpRequestMessage Parse(string fileContent)
    {
        IEnumerable<string?> lines = fileContent.Split(Environment.NewLine);

        var method = HttpMethod.Get;
        var requestUri = string.Empty;
        var httpClient = new HttpClient();
        var headers = httpClient.DefaultRequestHeaders;
        var content = new StringContent(string.Empty);
        var contentBuilder = new StringBuilder();
        var isBody = false;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                isBody = true;
                continue;
            }

            if (!isBody)
            {
                if (line.StartsWith(ParsingConstants.HttpGetMethodDirective, StringComparison.OrdinalIgnoreCase))
                {
                    method = HttpMethod.Get;
                    requestUri = line[(ParsingConstants.HttpGetMethodDirective.Length + 1)..].Trim();
                }
                else if (line.StartsWith(ParsingConstants.HttpPostMethodDirective, StringComparison.OrdinalIgnoreCase))
                {
                    method = HttpMethod.Post;
                    requestUri = line[(ParsingConstants.HttpPostMethodDirective.Length + 1)..].Trim();
                }
                else if (line.StartsWith(ParsingConstants.HttpPutMethodDirective, StringComparison.OrdinalIgnoreCase))
                {
                    method = HttpMethod.Put;
                    requestUri = line[(ParsingConstants.HttpPutMethodDirective.Length + 1)..].Trim();
                }
                else if (line.StartsWith(ParsingConstants.HttpDeleteMethodDirective, StringComparison.OrdinalIgnoreCase))
                {
                    method = HttpMethod.Delete;
                    requestUri = line[(ParsingConstants.HttpDeleteMethodDirective.Length + 1)..].Trim();
                }
                else if (line.Contains(':'))
                {
                    var headerParts = line.Split(':', 2);
                    var headerName = headerParts[0].Trim();
                    var headerValue = headerParts[1].Trim();
                    headers.TryAddWithoutValidation(headerName, headerValue);
                }
            }
            else
            {
                contentBuilder.AppendLine(line);
            }
        }

        if (contentBuilder.Length > 0)
        {
            content = new StringContent(contentBuilder.ToString().Trim(), Encoding.UTF8);
            if (headers.TryGetValues("Content-Type", out var contentType))
            {
                if (contentType?.Count() == 1)
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue(contentType.ToString()!);
                }
                else
                {
                    throw new InvalidOperationException("Unable to resolve Content-Type of the request.");
                }
            }
        }

        var requestMessage = new HttpRequestMessage(method, requestUri)
        {
            Content = method != HttpMethod.Get ? content : null
        };

        foreach (var header in headers)
        {
            if (requestMessage.Headers.Contains(header.Key))
            {
                requestMessage.Headers.Remove(header.Key);
            }
            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return requestMessage;
    }
}
