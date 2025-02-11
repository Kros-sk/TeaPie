namespace TeaPie.Http.Headers;

internal class ContentLanguageHeaderHandler : ContentHeaderHandler
{
    public override string HeaderName => "Content-Language";

    public override void SetHeader(string value, HttpRequestMessage requestMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, requestMessage.Content);
        requestMessage.Content.Headers.ContentLanguage.Add(value);
    }

    public override string GetHeader(HttpRequestMessage requestMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, requestMessage.Content);
        return string.Join(", ", requestMessage.Content.Headers.ContentLanguage);
    }

    public override string GetHeader(HttpResponseMessage responseMessage)
    {
        HeadersHandler.CheckIfContentExists(HeaderName, responseMessage.Content);
        return string.Join(", ", responseMessage.Content.Headers.ContentLanguage);
    }
}
