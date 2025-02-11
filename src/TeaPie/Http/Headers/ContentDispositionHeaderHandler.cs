using System.Net.Http.Headers;

namespace TeaPie.Http.Headers;

internal class ContentDispositionHeaderHandler : ContentHeaderHandler
{
    public override string HeaderName => "Content-Disposition";

    public override void SetHeader(string value, HttpRequestMessage requestMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, requestMessage.Content);
        requestMessage.Content.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse(value);
    }

    public override string GetHeader(HttpRequestMessage requestMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, requestMessage.Content);
        return requestMessage.Content.Headers.ContentDisposition?.ToString() ?? string.Empty;
    }

    public override string GetHeader(HttpResponseMessage responseMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, responseMessage.Content);
        return responseMessage.Content.Headers.ContentDisposition?.ToString() ?? string.Empty;
    }
}
