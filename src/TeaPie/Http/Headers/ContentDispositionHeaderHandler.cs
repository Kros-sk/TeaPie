using System.Net.Http.Headers;

namespace TeaPie.Http.Headers;

internal class ContentDispositionHeaderHandler : IHeaderHandler
{
    const string HeaderName = "Content-Disposition";

    public bool CanResolve(string name) => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase);

    public void SetHeader(string value, HttpRequestMessage requestMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, requestMessage.Content);
        requestMessage.Content.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse(value);
    }

    public string GetHeader(HttpRequestMessage requestMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, requestMessage.Content);
        return requestMessage.Content.Headers.ContentDisposition?.ToString() ?? string.Empty;
    }

    public string GetHeader(HttpResponseMessage responseMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, responseMessage.Content);
        return responseMessage.Content.Headers.ContentDisposition?.ToString() ?? string.Empty;
    }
}
