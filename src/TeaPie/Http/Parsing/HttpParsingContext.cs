using Polly.Retry;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using TeaPie.Testing;

namespace TeaPie.Http.Parsing;

internal class HttpParsingContext(HttpRequestHeaders defaultHeaders)
{
    public string RequestName { get; set; } = string.Empty;

    public bool IsMethodAndUriResolved { get; set; }
    public string RequestUri { get; set; } = string.Empty;
    public HttpMethod Method { get; set; } = HttpMethod.Get;

    private readonly Dictionary<string, string> _headers =
        defaultHeaders.ToDictionary(x => x.Key, y => string.Join(", ", y.Value));
    public IReadOnlyDictionary<string, string> Headers => _headers;

    public void AddHeader(string name, string value) => _headers[name] = value;

    public bool IsBody { get; set; }
    public StringBuilder BodyBuilder { get; } = new();

    public string RetryStrategyName { get; set; } = string.Empty;
    public IReadOnlyList<HttpStatusCode> RetryUntilStatusCodes { get; set; } = [];
    public RetryStrategyOptions<HttpResponseMessage>? ExplicitRetryStrategy { get; set; }
    public string RetryUntilTestPassTestName { get; set; } = string.Empty;

    public string AuthProviderName { get; set; } = string.Empty;

    private readonly Queue<TestDescription> _scheduledTests = [];
    public IReadOnlyList<TestDescription> Tests => [.. _scheduledTests];

    public void RegiterTest(TestDescription test) => _scheduledTests.Enqueue(test);
}
