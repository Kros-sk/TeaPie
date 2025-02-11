using System.Net.Http.Headers;

namespace TeaPie.Http.Headers;

internal class ContentTypeHeaderHandler : ContentHeaderHandler
{
    public override string HeaderName => "Content-Type";

    public override void SetHeader(string value, HttpRequestMessage requestMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, requestMessage.Content);
        requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(value);
    }

    public override string GetHeader(HttpRequestMessage requestMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, requestMessage.Content);
        return GetHeader(requestMessage.Content.Headers.ContentType);
    }

    public override string GetHeader(HttpResponseMessage responseMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, responseMessage.Content);
        return GetHeader(responseMessage.Content.Headers.ContentType);
    }

    private static string GetHeader(MediaTypeHeaderValue? contentType)
        => contentType is not null
            ? contentType.MediaType ?? string.Empty
            : string.Empty;
}
