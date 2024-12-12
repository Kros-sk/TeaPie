namespace TeaPie.Http.Headers;

internal interface IHeadersHandler
{
    void SetHeaders(HttpParsingContext parsingContext, HttpRequestMessage requestMessage);

    string GetHeader(string name, HttpRequestMessage requestMessage, string defaultValue = "");

    string GetHeader(string name, HttpResponseMessage responseMessage, string defaultValue = "");
}
