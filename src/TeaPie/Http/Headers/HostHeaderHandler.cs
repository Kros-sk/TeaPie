namespace TeaPie.Http.Headers;

internal class HostHeaderHandler : IHeaderHandler
{
    public string HeaderName => "Host";

    public bool CanResolve(string name, HttpRequestMessage responseMessage)
        => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase);

    public bool CanResolve(string name, HttpResponseMessage requestMessage)
        => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase);

    public void SetHeader(string value, HttpRequestMessage requestMessage)
       => requestMessage.Headers.Host = value;

    public string GetHeader(HttpRequestMessage requestMessage)
        => requestMessage.Headers.Host ?? string.Empty;

    public string GetHeader(HttpResponseMessage responseMessage)
        => string.Empty;
}
