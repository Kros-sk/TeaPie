using System.Diagnostics.CodeAnalysis;
using TeaPie.Http.Parsing;

namespace TeaPie.Http.Headers;

internal class HeadersHandler : IHeadersHandler
{
    private static readonly IHeaderHandler[] _handlers =
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

    #region Headers setting
    public void SetHeaders(HttpParsingContext parsingContext, HttpRequestMessage requestMessage)
    {
        foreach (var header in parsingContext.Headers)
        {
            HeaderNameValidator.CheckHeader(header.Key, string.Join(", ", header.Value));
            SetHeader(header, requestMessage);
        }
    }

    public void SetHeaders(HttpRequestMessage source, HttpRequestMessage target)
    {
        CopyBasicHeaders(source, target);
        CopySpecificHeaders(source, target);
    }

    private static void CopyBasicHeaders(HttpRequestMessage source, HttpRequestMessage target)
    {
        foreach (var header in source.Headers)
        {
            if (!target.Headers.TryAddWithoutValidation(header.Key, header.Value))
            {
                throw new InvalidOperationException($"Unable to add header with name '{header.Value}'");
            }
        }
    }

    private static void CopySpecificHeaders(HttpRequestMessage source, HttpRequestMessage target)
    {
        foreach (var handler in _handlers)
        {
            if (handler.CanResolve(handler.HeaderName, source))
            {
                var header = handler.GetHeader(source);

                if (!string.IsNullOrEmpty(header))
                {
                    HeaderNameValidator.CheckValue(header);
                    handler.SetHeader(header, target);
                }
            }
        }
    }

    private static void SetHeader(KeyValuePair<string, string> header, HttpRequestMessage requestMessage)
    {
        var handler = GetHandler(header.Key, requestMessage);
        handler.SetHeader(header.Value, requestMessage);
    }
    #endregion

    #region Headers getting
    public string GetHeader(string name, HttpRequestMessage requestMessage, string defaultValue = "")
        => GetHeader(name, GetHeaderFromRequest, requestMessage, defaultValue);

    public string GetHeader(string name, HttpResponseMessage responseMessage, string defaultValue = "")
        => GetHeader(name, GetHeaderFromResponse, responseMessage, defaultValue);

    public static string GetHeaderFromRequest(string name, HttpRequestMessage requestMessage, string defaultValue = "")
        => GetHeader(name, GetHeaderFromRequest, requestMessage, defaultValue);

    public static string GetHeaderFromResponse(string name, HttpResponseMessage responseMessage, string defaultValue = "")
        => GetHeader(name, GetHeaderFromResponse, responseMessage, defaultValue);

    private static string GetHeader<TMessage>(
        string name,
        Func<IHeaderHandler, TMessage, string> getter,
        TMessage message,
        string defaultValue = "")
        where TMessage : class
    {
        var handler = message switch
        {
            HttpResponseMessage responseMessage => GetHandler(name, responseMessage),
            HttpRequestMessage requestMessage => GetHandler(name, requestMessage),
            _ => throw new InvalidOperationException("Unable to get handler for message, which is neither 'HttpRequestMessage'" +
                "or 'HttpResponseMessage'")
        };

        var value = getter(handler, message);
        return !value.Equals(string.Empty) ? value : defaultValue;
    }

    private static string GetHeaderFromRequest(IHeaderHandler handler, HttpRequestMessage requestMessage)
        => handler.GetHeader(requestMessage);

    private static string GetHeaderFromResponse(IHeaderHandler handler, HttpResponseMessage responseMessage)
        => handler.GetHeader(responseMessage);
    #endregion

    private static IHeaderHandler GetHandler(string headerName, HttpResponseMessage responseMessage)
        => _handlers.FirstOrDefault(h => h.CanResolve(headerName, responseMessage), new DefaultHeaderHandler(headerName));

    private static IHeaderHandler GetHandler(string headerName, HttpRequestMessage requestMessage)
        => _handlers.FirstOrDefault(h => h.CanResolve(headerName, requestMessage), new DefaultHeaderHandler(headerName));

    public static void CheckIfContentExists(string headerName, [NotNull] HttpContent? content)
        => _ = content
            ?? throw new InvalidOperationException($"Unable to set header '{headerName}' when body's content is null.");
}
