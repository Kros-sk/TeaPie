using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace TeaPie.Http.Headers;

internal class HeadersHandler : IHeadersHandler
{
    private readonly IHeaderHandler[] _handlers =
    [
        new ContentTypeHeaderHandler(),
        new ContentDispositionHeaderHandler(),
        new ContentEncodingHeaderHandler(),
        new ContentLanguageHeaderHandler(),
        new AuthorizationHeaderHandler(),
        new UserAgentHeaderHandler(),
        new DateHeaderHandler(),
        new ConnectionHeaderHandler(),
        new HostHeaderHandler()
    ];

    public void SetHeaders(HttpParsingContext parsingContext, HttpRequestMessage requestMessage)
    {
        SetNormalHeaders(parsingContext, requestMessage);
        SetSpecialHeaders(parsingContext, requestMessage);
    }

    public string GetHeader(string name, HttpRequestMessage requestMessage, string defaultValue = "")
    {
        if (TryGetNormalHeader(name, requestMessage.Headers, out var found))
        {
            return found;
        }
        else if (TryGetHandler(name, out var handler))
        {
            var value = handler.GetHeader(requestMessage);
            return !value.Equals(string.Empty) ? value : defaultValue;
        }

        return defaultValue;
    }

    public string GetHeader(string name, HttpResponseMessage responseMessage, string defaultValue = "")
    {
        if (TryGetNormalHeader(name, responseMessage.Headers, out var found))
        {
            return found;
        }
        else if (TryGetHandler(name, out var handler))
        {
            var value = handler.GetHeader(responseMessage);
            return !value.Equals(string.Empty) ? value : defaultValue;
        }

        return defaultValue;
    }

    private static bool TryGetNormalHeader(string name, HttpHeaders headers, [NotNullWhen(true)] out string? found)
    {
        if (headers.TryGetValues(name, out var values))
        {
            found = string.Join(", ", values);
            return true;
        }
        else
        {
            found = null;
            return false;
        }
    }

    private static void SetNormalHeaders(HttpParsingContext parsingContext, HttpRequestMessage requestMessage)
    {
        foreach (var header in parsingContext.Headers)
        {
            if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value))
            {
                throw new InvalidOperationException($"Unable to set header '{header.Key} : {header.Value}'");
            }
        }
    }

    private void SetSpecialHeaders(HttpParsingContext parsingContext, HttpRequestMessage requestMessage)
    {
        foreach (var header in parsingContext.SpecialHeaders)
        {
            if (TryGetHandler(header.Key, out var handler))
            {
                handler.SetHeader(header.Value, requestMessage);
            }
            else
            {
                throw new InvalidOperationException($"Unable to set header '{header.Key} : {header.Value}'");
            }
        }
    }

    private bool TryGetHandler(string headerName, [NotNullWhen(true)] out IHeaderHandler? foundHandler)
    {
        foreach (var handler in _handlers)
        {
            if (handler.CanResolve(headerName))
            {
                foundHandler = handler;
                return true;
            }
        }

        foundHandler = null;
        return false;
    }

    public static void CheckIfContentExists(string headerName, [NotNull] HttpContent? content)
        => _ = content
            ?? throw new InvalidOperationException($"Unable to set header '{headerName}' when body's content is null.");
}
