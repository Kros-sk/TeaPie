using FluentAssertions;
using TeaPie.Http;

namespace TeaPie.Tests.Http;

public class HeadersResolverShould
{
    [Theory]
    [MemberData(nameof(GetNormalHeaderTestCases))]
    public void ResolveNormalHeadersCorrectly(
        string headerName,
        string headerValue,
        Action<HttpRequestMessage> assertHeader)
    {
        var parsingContext = new HttpParsingContext(new HttpClient().DefaultRequestHeaders);
        parsingContext.Headers.Add(headerName, headerValue);

        var requestMessage = new HttpRequestMessage();
        var headersResolver = new HeadersResolver();

        headersResolver.Resolve(parsingContext, requestMessage);

        assertHeader(requestMessage);
    }

    [Theory]
    [MemberData(nameof(GetSpecialHeaderTestCases))]
    public void ResolveHeadersCorrectly(
        string headerName,
        string headerValue,
        Action<HttpRequestMessage> assertHeader)
    {
        var parsingContext = new HttpParsingContext(new HttpClient().DefaultRequestHeaders)
        {
            SpecialHeaders = new()
            {
                { headerName, headerValue }
            }
        };

        var requestMessage = new HttpRequestMessage
        {
            Content = new StringContent("Hello World!")
        };

        var headersResolver = new HeadersResolver();

        headersResolver.Resolve(parsingContext, requestMessage);

        assertHeader(requestMessage);
    }

    public static IEnumerable<object[]> GetNormalHeaderTestCases()
    {
        yield return
        [
            "X-Custom-Header",
            "CustomValue",
            new Action<HttpRequestMessage>(request
                => request.Headers.GetValues("X-Custom-Header").Should().ContainSingle("CustomValue"))
        ];

        yield return
        [
            "Cache-Control",
            "no-cache",
            new Action<HttpRequestMessage>(request =>
            {
                request.Headers.CacheControl.Should().NotBeNull();
                request.Headers.CacheControl!.NoCache.Should().BeTrue();
            })
        ];
    }

    public static IEnumerable<object[]> GetSpecialHeaderTestCases()
    {
        yield return
        [
            "Content-Type",
            "application/json",
            new Action<HttpRequestMessage>(request =>
            {
                request.Content.Should().NotBeNull();
                request.Content!.Headers.ContentType.Should().NotBeNull();
                request.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
            })
        ];

        yield return
        [
            "Content-Disposition",
            "attachment; filename=\"example.txt\"",
            new Action<HttpRequestMessage>(request =>
            {
                request.Content.Should().NotBeNull();
                request.Content!.Headers.ContentDisposition.Should().NotBeNull();
                request.Content.Headers.ContentDisposition!.DispositionType.Should().Be("attachment");
                request.Content.Headers.ContentDisposition.FileName.Should().Be("\"example.txt\"");
            })
        ];

        yield return
        [
            "Content-Encoding",
            "gzip",
            new Action<HttpRequestMessage>(request =>
            {
                request.Content.Should().NotBeNull();
                request.Content!.Headers.ContentEncoding.Should().ContainSingle("gzip");
            })
        ];

        yield return
        [
            "Content-Language",
            "en-US",
            new Action<HttpRequestMessage>(request =>
            {
                request.Content.Should().NotBeNull();
                request.Content!.Headers.ContentLanguage.Should().ContainSingle("en-US");
            })
        ];

        yield return
        [
            "Authorization",
            "Bearer my-token",
            new Action<HttpRequestMessage>(request =>
            {
                request.Headers.Authorization.Should().NotBeNull();
                request.Headers.Authorization!.Scheme.Should().Be("Bearer");
                request.Headers.Authorization.Parameter.Should().Be("my-token");
            })
        ];

        yield return
        [
            "User-Agent",
            "MyApp/1.0",
            new Action<HttpRequestMessage>(request =>
            {
                request.Headers.UserAgent.Should().NotBeEmpty();
                request.Headers.UserAgent.ToString().Should().Contain("MyApp/1.0");
            })
        ];

        yield return
        [
            "Date",
            "Tue, 15 Nov 1994 08:12:31 GMT",
            new Action<HttpRequestMessage>(request =>
            {
                request.Headers.Date.Should().NotBeNull();
                request.Headers.Date!.Value.UtcDateTime.Should().Be(new DateTime(1994, 11, 15, 8, 12, 31, DateTimeKind.Utc));
            })
        ];

        yield return
        [
            "Connection",
            "close",
            new Action<HttpRequestMessage>(request => request.Headers.ConnectionClose.Should().BeTrue())
        ];

        yield return
        [
            "Host",
            "example.com",
            new Action<HttpRequestMessage>(request => request.Headers.Host.Should().Be("example.com"))
        ];
    }
}
