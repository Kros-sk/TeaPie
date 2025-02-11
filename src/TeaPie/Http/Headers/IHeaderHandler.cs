namespace TeaPie.Http.Headers;

internal interface IHeaderHandler
{
    string HeaderName { get; }

    bool CanResolve(string name, HttpRequestMessage responseMessage);

    bool CanResolve(string name, HttpResponseMessage requestMessage);

    void SetHeader(string value, HttpRequestMessage requestMessage);

    string GetHeader(HttpRequestMessage responseMessage);

    string GetHeader(HttpResponseMessage requestMessage);
}
