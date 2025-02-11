namespace TeaPie.Http.Headers;

internal class ContentEncodingHeaderHandler : IHeaderHandler
{
    public string HeaderName => "Content-Encoding";

    public bool CanResolve(string name, HttpRequestMessage responseMessage)
        => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase) && responseMessage.Content is not null;

    public bool CanResolve(string name, HttpResponseMessage requestMessage)
        => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase) && requestMessage.Content is not null;

    public void SetHeader(string value, HttpRequestMessage requestMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, requestMessage.Content);
        requestMessage.Content.Headers.ContentEncoding.Add(value);
    }

    public string GetHeader(HttpRequestMessage requestMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, requestMessage.Content);
        return string.Join(", ", requestMessage.Content.Headers.ContentEncoding);
    }

    public string GetHeader(HttpResponseMessage responseMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, responseMessage.Content);
        return string.Join(", ", responseMessage.Content.Headers.ContentEncoding);
    }
}
