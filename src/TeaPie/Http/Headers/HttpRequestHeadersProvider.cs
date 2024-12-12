using System.Net.Http.Headers;

namespace TeaPie.Http.Headers;

internal interface IHttpRequestHeadersProvider
{
    HttpRequestHeaders GetDefaultHeaders();
}

internal class HttpRequestHeadersProvider(IHttpClientFactory clientFactory) : IHttpRequestHeadersProvider
{
    private readonly IHttpClientFactory _clientFactory = clientFactory;
    private HttpRequestHeaders? _defaultHeaders;

    public HttpRequestHeaders GetDefaultHeaders()
    {
        if (_defaultHeaders is null)
        {
            using var client = _clientFactory.CreateClient(nameof(HttpRequestHeadersProvider));
            _defaultHeaders = client.DefaultRequestHeaders;
        }

        return _defaultHeaders;
    }
}
