using System.Net.Http.Headers;

namespace TeaPie.Http.Headers;

internal class DefaultHeaderHandler(string headerName) : IHeaderHandler
{
    public string HeaderName { get; } = headerName;

    public void SetHeader(string value, HttpRequestMessage requestMessage)
    {
        if (!requestMessage.Headers.TryAddWithoutValidation(HeaderName, value))
        {
            throw new InvalidOperationException($"Unable to resolve '{HeaderName}' header with the value '{value}'.");
        }
    }

    public string GetHeader(HttpRequestMessage requestMessage)
        => GetHeader(requestMessage.Headers);

    public string GetHeader(HttpResponseMessage responseMessage)
        => GetHeader(responseMessage.Headers);

    private string GetHeader(HttpHeaders headers)
        => headers.TryGetValues(HeaderName, out var values)
            ? string.Join(", ", values)
            : string.Empty;
}
