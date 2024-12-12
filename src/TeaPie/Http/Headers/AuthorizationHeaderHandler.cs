using System.Net.Http.Headers;

namespace TeaPie.Http.Headers;

internal class AuthorizationHeaderHandler : IHeaderHandler
{
    const string HeaderName = "Authorization";

    public bool CanResolve(string name) => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase);

    public void SetHeader(string value, HttpRequestMessage requestMessage)
    {
        var parts = value.Split(' ', 2);
        if (parts.Length != 2)
        {
            throw new InvalidOperationException($"Invalid format for '{HeaderName}' header. Expected: 'Scheme Value'.");
        }

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(parts[0], parts[1]);
    }

    public string GetHeader(HttpRequestMessage requestMessage)
        => requestMessage.Headers.Authorization?.ToString() ?? string.Empty;

    public string GetHeader(HttpResponseMessage responseMessage) => string.Empty;
}
