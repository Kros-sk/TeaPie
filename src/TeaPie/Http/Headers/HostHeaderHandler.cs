namespace TeaPie.Http.Headers;

internal class HostHeaderHandler : IHeaderHandler
{
    public string HeaderName => "Host";

    public void SetHeader(string value, HttpRequestMessage requestMessage)
       => requestMessage.Headers.Host = value;

    public string GetHeader(HttpRequestMessage requestMessage)
        => requestMessage.Headers.Host ?? string.Empty;

    public string GetHeader(HttpResponseMessage responseMessage)
        => string.Empty;
}
