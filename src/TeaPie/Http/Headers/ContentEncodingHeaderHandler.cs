namespace TeaPie.Http.Headers;

internal class ContentEncodingHeaderHandler : ContentHeaderHandler
{
    public override string HeaderName => "Content-Encoding";

    public override void SetHeader(string value, HttpRequestMessage requestMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, requestMessage.Content);
        requestMessage.Content.Headers.ContentEncoding.Add(value);
    }

    public override string GetHeader(HttpRequestMessage requestMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, requestMessage.Content);
        return string.Join(", ", requestMessage.Content.Headers.ContentEncoding);
    }

    public override string GetHeader(HttpResponseMessage responseMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, responseMessage.Content);
        return string.Join(", ", responseMessage.Content.Headers.ContentEncoding);
    }
}
