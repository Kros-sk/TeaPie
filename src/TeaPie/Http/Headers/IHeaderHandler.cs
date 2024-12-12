namespace TeaPie.Http.Headers;

internal interface IHeaderHandler
{
    bool CanResolve(string name);

    void SetHeader(string value, HttpRequestMessage requestMessage);

    string GetHeader(HttpRequestMessage responseMessage);

    string GetHeader(HttpResponseMessage requestMessage);
}
