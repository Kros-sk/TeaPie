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
        var isMethodAndUriResolved = false;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                isBody = true;
                continue;
            }

            if (!isBody)
            {
                if (!line.TrimStart().StartsWith(ParsingConstants.HttpCommentPrefix))
                {
                    if (line.Contains(ParsingConstants.HttpDirectivePrefix))
                    {
                        // TODO: Handle directives
                    }
                    else if (!isMethodAndUriResolved)
                    {
                        (method, requestUri) = ResolveMethodAndUri(line);
                        isMethodAndUriResolved = true;
                    }
                    else if (line.Contains(ParsingConstants.HttpHeaderSeparator))
                    {
                        ResolveHeader(line, headers);
                    }
                }
            }
            else
            {
                contentBuilder.AppendLine(line);
            }
        }

        var bodyContent = contentBuilder.ToString().Trim();
        if (!string.IsNullOrEmpty(bodyContent))
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

        var requestMessage = new HttpRequestMessage(method, requestUri);
        if (!string.IsNullOrEmpty(bodyContent))
        {
            requestMessage.Content = content;
        }

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

    private static (HttpMethod, string) ResolveMethodAndUri(string line)
    {
        var httpMethod = HttpMethod.Get;
        var requestUri = string.Empty;

        var splitted = line.TrimStart().Split(' ');
        if (splitted.Length > 0)
        {
            var keyWord = splitted[0].TrimEnd();
            if (ParsingConstants.HttpMethodsMap.TryGetValue(keyWord, out var method))
            {
                httpMethod = method;
                requestUri = line[(keyWord.Length + 1)..].Trim();
            }
            else
            {
                throw new InvalidOperationException("Error while parsing HTTP method and Uri line.");
            }
        }

        return (httpMethod, requestUri);
    }

    private static void ResolveHeader(string line, HttpRequestHeaders headers)
    {
        var headerParts = line.Split(ParsingConstants.HttpHeaderSeparator, 2);
        var headerName = headerParts[0].Trim();
        var headerValue = headerParts[1].Trim();
        headers.TryAddWithoutValidation(headerName, headerValue);
    }
}
