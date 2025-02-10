using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using TeaPie.Http;
using TeaPie.Http.Headers;
using TeaPie.Http.Retrying;
using TeaPie.StructureExploration;
using TeaPie.Variables;
using File = TeaPie.StructureExploration.File;

namespace TeaPie.Tests.Http;

public class HttpRequestParserShould
{
    private readonly Uri _baseRequestUri = new("https://jsonplaceholder.typicode.com/posts");
    private readonly Uri _specificRequestUri = new("https://jsonplaceholder.typicode.com/posts/1");
    private readonly Uri _traceRequestUri = new("https://postman-echo.com/trace");

    [Fact]
    public async Task ParseSimpleRequestCorrectly()
    {
        var parsed = await GetParsedContext(RequestsIndex.SimpleRequestPath);
        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Get, _specificRequestUri, false);
    }

    [Fact]
    public async Task ParseRequestWithCommentCorrectly()
    {
        var parsed = await GetParsedContext(RequestsIndex.RequestWithCommentPath);
        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Get, _specificRequestUri, false);
    }

    [Fact]
    public async Task ParseRequestWithCommentsCorrectly()
    {
        var parsed = await GetParsedContext(RequestsIndex.RequestWithCommentsPath);
        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Get, _specificRequestUri, false);
    }

    [Fact]
    public async Task ParseRequestWithCommentsAllOverFileCorrectly()
    {
        var parsed = await GetParsedContext(RequestsIndex.RequestWithCommentsAllOverFile);

        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Post, _baseRequestUri, true);
        await CheckBody(parsed, "\"title\": \"foo\"", "\"body\": \"bar\"", "\"userId\": 1");

        parsed.Request!.Headers.UserAgent.ToString().Should().Be("UnitTest/1.0");
        parsed.Request.Headers.GetValues("X-Test-Case").Should().Contain("RequestWithCommentsBodyAndHeaders");

        parsed.Name.Should().BeEquivalentTo("FullyStructuredRequest");
    }

    [Fact]
    public async Task ParseRequestWithNameCorrectly()
    {
        var parsed = await GetParsedContext(RequestsIndex.RequestWithNamePath);
        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Get, _specificRequestUri, false);

        parsed.Name.Should().BeEquivalentTo("NamedRequest");
    }

    [Fact]
    public async Task ParseRequestWithJsonBodyCorrectly()
    {
        var parsed = await GetParsedContext(RequestsIndex.RequestWithJsonBodyPath);

        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Post, _baseRequestUri, true);
        await CheckBody(parsed, "\"title\": \"foo\"", "\"body\": \"bar\"", "\"userId\": 1");
    }

    [Fact]
    public async Task ParseRequestWithHeaderCorrectly()
    {
        var parsed = await GetParsedContext(RequestsIndex.RequestWithHeaderPath);

        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Get, _specificRequestUri, false);

        parsed.Request!.Headers.UserAgent.ToString().Should().Be("UnitTest/1.0");
    }

    [Fact]
    public async Task ParseRequestWithHeadersCorrectly()
    {
        var parsed = await GetParsedContext(RequestsIndex.RequestWithHeadersPath);

        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Get, _specificRequestUri, false);

        parsed.Request!.Headers.UserAgent.ToString().Should().Be("UnitTest/1.0");
        parsed.Request.Headers.Accept.ToString().Should().Contain("application/json");
    }

    [Fact]
    public async Task ParseRequestWithBodyAndHeaderCorrectly()
    {
        var parsed = await GetParsedContext(RequestsIndex.RequestWithBodyAndHeadersPath);

        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Post, _baseRequestUri, true);
        await CheckBody(parsed, "\"title\": \"foo\"");
    }

    [Fact]
    public async Task ParseRequestWithBodyAndHeadersCorrectly()
    {
        var parsed = await GetParsedContext(RequestsIndex.RequestWithBodyAndHeadersPath);

        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Post, _baseRequestUri, true);
        await CheckBody(parsed, "\"title\": \"foo\"", "\"body\": \"bar\"", "\"userId\": 1");

        parsed.Request!.Headers.UserAgent.ToString().Should().Be("UnitTest/1.0");
        parsed.Request!.Headers.GetValues("X-Test-Case").Should().Contain("RequestWithBodyAndHeaders");
    }

    [Fact]
    public async Task ParseFullyStructuredRequestCorrectly()
    {
        var parsed = await GetParsedContext(RequestsIndex.RequestWithFullStructure);

        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Post, _baseRequestUri, true);
        await CheckBody(parsed, "\"title\": \"foo\"", "\"body\": \"bar\"", "\"userId\": 1");

        parsed.Request!.Headers.UserAgent.ToString().Should().Be("UnitTest/1.0");
        parsed.Request.Headers.GetValues("X-Test-Case").Should().Contain("RequestWithCommentsBodyAndHeaders");

        parsed.Name.Should().BeEquivalentTo("FullyStructuredRequest");
    }

    [Fact]
    public async Task ParseGetRequestCorrectly()
    {
        var parsed = await GetParsedContext(RequestsIndex.PlainGetRequestPath);
        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Get, _baseRequestUri, false);
    }

    [Fact]
    public async Task ParsePostRequestCorrectly()
    {
        var parsed = await GetParsedContext(RequestsIndex.PlainPostRequestPath);

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
        var parsed = await GetParsedContext(RequestsIndex.PlainPutRequestPath);

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
        var parsed = await GetParsedContext(RequestsIndex.PlainPatchRequestPath);

        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Patch, _specificRequestUri, true);
        await CheckBody(parsed, "\"title\": \"patched title\"");
    }

    [Fact]
    public async Task ParseDeleteRequestCorrectly()
    {
        var parsed = await GetParsedContext(RequestsIndex.PlainDeleteRequestPath);
        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Delete, _specificRequestUri, false);
    }

    [Fact]
    public async Task ParseHeadRequestCorrectly()
    {
        var parsed = await GetParsedContext(RequestsIndex.PlainHeadRequestPath);
        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Head, _specificRequestUri, false);
    }

    [Fact]
    public async Task ParseOptionsRequestCorrectly()
    {
        var parsed = await GetParsedContext(RequestsIndex.PlainOptionsRequestPath);
        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Options, _specificRequestUri, false);
    }

    [Fact]
    public async Task ParseTraceRequestCorrectly()
    {
        var parsed = await GetParsedContext(RequestsIndex.PlainTraceRequestPath);
        CheckMethodUriAndExistenceOfContent(parsed, HttpMethod.Trace, _traceRequestUri, false);
    }

    private static async Task<RequestExecutionContext> GetParsedContext(string path)
    {
        var services = new ServiceCollection();
        services.AddHttpClient();

        var serviceProvider = services.BuildServiceProvider();

        var clientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var headersProvider = new HttpRequestHeadersProvider(clientFactory);

        var variables = new global::TeaPie.Variables.Variables();
        var variablesResolver = new VariablesResolver(variables, serviceProvider);
        var headersResolver = new HeadersHandler();

        var parser = new HttpRequestParser(
            headersProvider, variablesResolver, headersResolver, Substitute.For<IRetryingStrategiesRegistry>());

        var folder =
            new Folder(RequestsIndex.RootFolderFullPath, RequestsIndex.RootFolderName, RequestsIndex.RootFolderName, null);
        var file = File.Create(path, folder);

        var requestContext = new RequestExecutionContext(file)
        {
            RawContent = await System.IO.File.ReadAllTextAsync(path)
        };

        parser.Parse(requestContext);
        return requestContext;
    }

    private static void CheckMethodUriAndExistenceOfContent(
        RequestExecutionContext parsed,
        HttpMethod method,
        Uri uri,
        bool shouldHaveContent)
    {
        parsed.Request.Should().NotBeNull();

        parsed.Request!.Method.Should().Be(method);
        parsed.Request.RequestUri.Should().BeEquivalentTo(uri);
        if (shouldHaveContent)
        {
            parsed.Request.Content.Should().NotBeNull();
        }
        else
        {
            parsed.Request.Content.Should().BeNull();
        }
    }

    private static async Task CheckBody(RequestExecutionContext parsed, params string[] shouldContainPhrases)
    {
        parsed.Request.Should().NotBeNull();
        var body = await parsed.Request!.Content!.ReadAsStringAsync();

        foreach (var phrase in shouldContainPhrases)
        {
            body.Should().Contain(phrase);
        }
    }
}
