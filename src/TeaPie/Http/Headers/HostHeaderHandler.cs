namespace TeaPie.Http.Headers;

internal class HostHeaderHandler : IHeaderHandler
{
    const string HeaderName = "Host";

    public bool CanResolve(string name) => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase);

    public void SetHeader(string value, HttpRequestMessage requestMessage)
       => requestMessage.Headers.Host = value;

    public string GetHeader(HttpRequestMessage requestMessage)
        => requestMessage.Headers.Host ?? string.Empty;

    public string GetHeader(HttpResponseMessage responseMessage)
        => string.Empty;
}
