namespace TeaPie.Http.Headers;

internal class ContentEncodingHeaderHandler : IHeaderHandler
{
    const string HeaderName = "Content-Encoding";

    public bool CanResolve(string name) => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase);

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
