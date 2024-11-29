using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TeaPie.Http;
using TeaPie.Variables;

namespace TeaPie.Tests.Http;

public class HttpFileParserShould
{
    private readonly Uri _baseRequestUri = new("https://jsonplaceholder.typicode.com/posts");
    private readonly Uri _specificRequestUri = new("https://jsonplaceholder.typicode.com/posts/1");
    private readonly Uri _traceRequestUri = new("https://postman-echo.com/trace");

    [Fact]
    public async Task ParseSimpleRequestCorrectly()
    {
        var parsed = await GetParsedFile(RequestsIndex.SimpleRequestPath);
        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Get, _specificRequestUri, false);
    }

    [Fact]
    public async Task ParseRequestWithCommentCorrectly()
    {
        var parsed = await GetParsedFile(RequestsIndex.RequestWithCommentPath);
        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Get, _specificRequestUri, false);
    }

    [Fact]
    public async Task ParseRequestWithCommentsCorrectly()
    {
        var parsed = await GetParsedFile(RequestsIndex.RequestWithCommentsPath);
        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Get, _specificRequestUri, false);
    }

    [Fact]
    public async Task ParseRequestWithJsonBodyCorrectly()
    {
        var parsed = await GetParsedFile(RequestsIndex.RequestWithJsonBodyPath);

        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Post, _baseRequestUri, true);
        await CheckBody(parsed, "\"title\": \"foo\"", "\"body\": \"bar\"", "\"userId\": 1");
    }

    [Fact]
    public async Task ParseRequestWithHeaderCorrectly()
    {
        var parsed = await GetParsedFile(RequestsIndex.RequestWithHeaderPath);

        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Get, _specificRequestUri, false);

        parsed.Headers.UserAgent.ToString().Should().Be("UnitTest/1.0");
    }

    [Fact]
    public async Task ParseRequestWithHeadersCorrectly()
    {
        var parsed = await GetParsedFile(RequestsIndex.RequestWithHeadersPath);

        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Get, _specificRequestUri, false);

        parsed.Headers.UserAgent.ToString().Should().Be("UnitTest/1.0");
        parsed.Headers.Accept.ToString().Should().Contain("application/json");
    }

    [Fact]
    public async Task ParseRequestWithBodyAndHeaderCorrectly()
    {
        var parsed = await GetParsedFile(RequestsIndex.RequestWithBodyAndHeadersPath);

        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Post, _baseRequestUri, true);
        await CheckBody(parsed, "\"title\": \"foo\"");
    }

    [Fact]
    public async Task ParseRequestWithBodyAndHeadersCorrectly()
    {
        var parsed = await GetParsedFile(RequestsIndex.RequestWithBodyAndHeadersPath);

        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Post, _baseRequestUri, true);
        await CheckBody(parsed, "\"title\": \"foo\"", "\"body\": \"bar\"", "\"userId\": 1");

        parsed.Headers.UserAgent.ToString().Should().Be("UnitTest/1.0");
        parsed.Headers.GetValues("X-Test-Case").Should().Contain("RequestWithBodyAndHeaders");
    }

    [Fact]
    public async Task ParseRequestWithCommentsBodyAndHeadersCorrectly()
    {
        var parsed = await GetParsedFile(RequestsIndex.RequestWithCommentsBodyAndHeadersPath);

        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Post, _baseRequestUri, true);
        await CheckBody(parsed, "\"title\": \"foo\"", "\"body\": \"bar\"", "\"userId\": 1");

        parsed.Headers.UserAgent.ToString().Should().Be("UnitTest/1.0");
        parsed.Headers.GetValues("X-Test-Case").Should().Contain("RequestWithCommentsBodyAndHeaders");
    }

    [Fact]
    public async Task ParseGetRequestCorrectly()
    {
        var parsed = await GetParsedFile(RequestsIndex.PlainGetRequestPath);
        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Get, _baseRequestUri, false);
    }

    [Fact]
    public async Task ParsePostRequestCorrectly()
    {
        var parsed = await GetParsedFile(RequestsIndex.PlainPostRequestPath);

        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Post, _baseRequestUri, true);
        await CheckBody(parsed, "{" + Environment.NewLine +
            "  \"title\": \"foo\"," + Environment.NewLine +
            "  \"body\": \"bar\"," + Environment.NewLine +
            "  \"userId\": 1" + Environment.NewLine +
            "}");
    }

    [Fact]
    public async Task ParsePutRequestCorrectly()
    {
        var parsed = await GetParsedFile(RequestsIndex.PlainPutRequestPath);

        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Put, _specificRequestUri, true);
        await CheckBody(parsed, "{" + Environment.NewLine +
            "  \"id\": 1," + Environment.NewLine +
            "  \"title\": \"updated title\"," + Environment.NewLine +
            "  \"body\": \"updated body\"," + Environment.NewLine +
            "  \"userId\": 1" + Environment.NewLine +
            "}");
    }

    [Fact]
    public async Task ParsePatchRequestCorrectly()
    {
        var parsed = await GetParsedFile(RequestsIndex.PlainPatchRequestPath);

        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Patch, _specificRequestUri, true);
        await CheckBody(parsed, "\"title\": \"patched title\"");
    }

    [Fact]
    public async Task ParseDeleteRequestCorrectly()
    {
        var parsed = await GetParsedFile(RequestsIndex.PlainDeleteRequestPath);
        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Delete, _specificRequestUri, false);
    }

    [Fact]
    public async Task ParseHeadRequestCorrectly()
    {
        var parsed = await GetParsedFile(RequestsIndex.PlainHeadRequestPath);
        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Head, _specificRequestUri, false);
    }

    [Fact]
    public async Task ParseOptionsRequestCorrectly()
    {
        var parsed = await GetParsedFile(RequestsIndex.PlainOptionsRequestPath);
        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Options, _specificRequestUri, false);
    }

    [Fact]
    public async Task ParseTraceRequestCorrectly()
    {
        var parsed = await GetParsedFile(RequestsIndex.PlainTraceRequestPath);
        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Trace, _traceRequestUri, false);
    }

    private static async Task<HttpRequestMessage> GetParsedFile(string path)
    {
        var services = new ServiceCollection();
        services.AddHttpClient();

        var serviceProvider = services.BuildServiceProvider();

        var clientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var headersProvider = new HttpRequestHeadersProvider(clientFactory);

        var variables = new global::TeaPie.Variables.Variables();
        var resolver = new VariablesResolver(variables);

        var parser = new HttpFileParser(headersProvider, resolver);
        return parser.Parse(await File.ReadAllTextAsync(path));
    }

    private static void CheckMethodUriAndExistenceOfContent(
        HttpRequestMessage parsed,
        HttpMethod method,
        Uri uri,
        bool shouldHaveContent)
    {
        parsed.Method.Should().Be(method);
        parsed.RequestUri.Should().BeEquivalentTo(uri);
        if (shouldHaveContent)
        {
            parsed.Content.Should().NotBeNull();
        }
        else
        {
            parsed.Content.Should().BeNull();
        }
    }

    private static async Task CheckBody(HttpRequestMessage parsed, params string[] shouldContainPhrases)
    {
        var body = await parsed.Content!.ReadAsStringAsync();

        foreach (var phrase in shouldContainPhrases)
        {
            body.Should().Contain(phrase);
        }
    }
}
