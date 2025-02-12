using TeaPie.Http.Parsing;

namespace TeaPie.Http.Headers;

internal interface IHeadersHandler
{
    void SetHeaders(HttpParsingContext parsingContext, HttpRequestMessage requestMessage);

    void SetHeaders(HttpRequestMessage source, HttpRequestMessage target);

    string GetHeader(string name, HttpRequestMessage requestMessage, string defaultValue = "");

    string GetHeader(string name, HttpResponseMessage responseMessage, string defaultValue = "");
}
