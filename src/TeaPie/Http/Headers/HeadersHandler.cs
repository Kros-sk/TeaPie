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

    public void SetHeaders(HttpRequestMessage source, HttpRequestMessage target)
    {
        CopySpecificHeaders(source, target);
        CopyBasicHeaders(source, target);
    }

    private static void CopyBasicHeaders(HttpRequestMessage source, HttpRequestMessage target)
    {
        foreach (var header in source.Headers)
        {
            if (!target.Headers.TryAddWithoutValidation(header.Key, header.Value))
            {
                throw new InvalidOperationException($"Unable to add header with name {header.Value}");
            }
        }
    }

    private void CopySpecificHeaders(HttpRequestMessage source, HttpRequestMessage target)
    {
        foreach (var handler in _handlers)
        {
            var header = handler.GetHeader(source);
            HeaderNameValidator.CheckValue(header);

            if (!string.IsNullOrEmpty(header))
            {
                handler.SetHeader(header, target);
            }
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

    private string GetHeader<TMessage>(
        string name,
        Func<IHeaderHandler, TMessage, string> getter,
        TMessage message,
        string defaultValue = "")
        where TMessage : class
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
