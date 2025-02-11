namespace TeaPie.Http.Headers;

internal class UserAgentHeaderHandler : IHeaderHandler
{
    public string HeaderName => "User-Agent";

    public void SetHeader(string value, HttpRequestMessage requestMessage)
        => requestMessage.Headers.UserAgent.ParseAdd(value);

    public string GetHeader(HttpRequestMessage requestMessage)
        => string.Join(" ", requestMessage.Headers.UserAgent);

    public string GetHeader(HttpResponseMessage responseMessage)
        => string.Empty;
}
