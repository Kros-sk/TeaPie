namespace TeaPie.Http.Headers;

internal interface IHeaderHandler
{
    string HeaderName { get; }

    bool CanResolve(string name, HttpRequestMessage responseMessage)
        => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase);

    bool CanResolve(string name, HttpResponseMessage requestMessage)
        => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase);

    void SetHeader(string value, HttpRequestMessage requestMessage);

    string GetHeader(HttpRequestMessage responseMessage);

    string GetHeader(HttpResponseMessage requestMessage);
}
