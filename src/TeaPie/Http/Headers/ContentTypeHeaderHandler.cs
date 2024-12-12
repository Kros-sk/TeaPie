using System.Net.Http.Headers;

namespace TeaPie.Http.Headers;

internal class ContentTypeHeaderHandler : IHeaderHandler
{
    const string HeaderName = "Content-Type";

    public bool CanResolve(string name) => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase);

    public void SetHeader(string value, HttpRequestMessage requestMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, requestMessage.Content);
        requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(value);
    }

    public string GetHeader(HttpRequestMessage requestMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, requestMessage.Content);
        return GetHeader(requestMessage.Content.Headers.ContentType);
    }

    public string GetHeader(HttpResponseMessage responseMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, responseMessage.Content);
        return GetHeader(responseMessage.Content.Headers.ContentType);
    }

    private static string GetHeader(MediaTypeHeaderValue? contentType)
        => contentType is not null
            ? contentType.MediaType ?? string.Empty
            : string.Empty;
}
