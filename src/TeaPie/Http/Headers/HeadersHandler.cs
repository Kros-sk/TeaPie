using System.Diagnostics.CodeAnalysis;

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

    #region Headers setting
    public void SetHeaders(HttpParsingContext parsingContext, HttpRequestMessage requestMessage)
    {
        foreach (var header in parsingContext.Headers)
        {
            HeaderNameValidator.CheckHeader(header.Key, string.Join(", ", header.Value));
            SetHeader(header, requestMessage);
        }
    }

    private void SetHeader(KeyValuePair<string, string> header, HttpRequestMessage requestMessage)
    {
        var handler = GetHandler(header.Key);
        handler.SetHeader(header.Value, requestMessage);
    }
    #endregion

    #region Headers getting
    public string GetHeader(string name, HttpRequestMessage requestMessage, string defaultValue = "")
        => GetHeader(name, GetHeaderFromRequest, requestMessage, defaultValue);

    public string GetHeader(string name, HttpResponseMessage responseMessage, string defaultValue = "")
        => GetHeader(name, GetHeaderFromResponse, responseMessage, defaultValue);

    private string GetHeader<T>(string name, Func<IHeaderHandler, T, string> getter, T message, string defaultValue = "")
        where T : class
    {
        var handler = GetHandler(name);

        var value = getter(handler, message);
        return !value.Equals(string.Empty) ? value : defaultValue;
    }

    private string GetHeaderFromRequest(IHeaderHandler handler, HttpRequestMessage requestMessage)
        => handler.GetHeader(requestMessage);

    private string GetHeaderFromResponse(IHeaderHandler handler, HttpResponseMessage responseMessage)
        => handler.GetHeader(responseMessage);
    #endregion

    private IHeaderHandler GetHandler(string headerName)
        => _handlers.FirstOrDefault(h => h.CanResolve(headerName), new DefaultHeaderHandler(headerName));

    public static void CheckIfContentExists(string headerName, [NotNull] HttpContent? content)
        => _ = content
            ?? throw new InvalidOperationException($"Unable to set header '{headerName}' when body's content is null.");
}
