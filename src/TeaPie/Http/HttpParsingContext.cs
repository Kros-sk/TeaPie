using System.Net.Http.Headers;
using System.Text;

namespace TeaPie.Http;

internal class HttpParsingContext(HttpRequestHeaders defaultHeaders)
{
    public string RequestName { get; set; } = string.Empty;
    public HttpMethod Method { get; set; } = HttpMethod.Get;
    public string RequestUri { get; set; } = string.Empty;

    private readonly Dictionary<string, string> _headers =
        defaultHeaders.ToDictionary(x => x.Key, y => string.Join(", ", y.Value));
    public IReadOnlyDictionary<string, string> Headers => _headers;

    public void AddHeader(string name, string value) => _headers[name] = value;

    public StringBuilder BodyBuilder { get; } = new();
    public bool IsBody { get; set; }
    public bool IsMethodAndUriResolved { get; set; }
}
