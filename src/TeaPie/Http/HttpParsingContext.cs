using System.Net.Http.Headers;
using System.Text;

namespace TeaPie.Http;

internal class HttpParsingContext(HttpRequestHeaders defaultHeaders)
{
    public string RequestName { get; set; } = string.Empty;
    public HttpMethod Method { get; set; } = HttpMethod.Get;
    public string RequestUri { get; set; } = string.Empty;
    public HttpRequestHeaders Headers { get; } = defaultHeaders;
    public Dictionary<string, string> ContentHeaders = [];
    public Dictionary<string, string> SpecialHeaders = [];
    public StringBuilder BodyBuilder { get; } = new();
    public bool IsBody { get; set; }
    public bool IsMethodAndUriResolved { get; set; }
}
