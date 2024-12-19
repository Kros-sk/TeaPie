using System.Net.Http.Headers;

namespace TeaPie.Http.Headers;

internal class DefaultHeaderHandler(string headerName) : IHeaderHandler
{
    private readonly string _headerName = headerName;

    public bool CanResolve(string name) => name.Equals(_headerName, StringComparison.OrdinalIgnoreCase);

    public void SetHeader(string value, HttpRequestMessage requestMessage)
    {
        if (!requestMessage.Headers.TryAddWithoutValidation(_headerName, value))
        {
            throw new InvalidOperationException($"Unable to resolve '{_headerName}' header with the value '{value}'.");
        }
    }

    public string GetHeader(HttpRequestMessage requestMessage)
        => GetHeader(requestMessage.Headers);

    public string GetHeader(HttpResponseMessage responseMessage)
        => GetHeader(responseMessage.Headers);

    private string GetHeader(HttpHeaders headers)
        => headers.TryGetValues(_headerName, out var values)
            ? string.Join(", ", values)
            : string.Empty;
}
