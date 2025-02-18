using FluentAssertions;
using System.Data;
using System.Net.Http.Headers;
using TeaPie.Http.Headers;
using TeaPie.Http.Parsing;

namespace TeaPie.Tests.Http.Headers;

public class HeadersHandlerShould
{
    private readonly HeadersHandler _headersHandler;
    private readonly HttpRequestHeaders _defaultHeaders;

    public HeadersHandlerShould()
    {
        _headersHandler = new HeadersHandler();
        _defaultHeaders = new HttpClient().DefaultRequestHeaders;
    }

    [Fact]
    public void SetNormalHeadersCorrectly()
    {
        var parsingContext = new HttpParsingContext(_defaultHeaders);
        parsingContext.AddHeader("X-Custom-Header", "CustomValue");
        parsingContext.AddHeader("Cache-Control", "no-cache");

        var requestMessage = new HttpRequestMessage();

        _headersHandler.SetHeaders(parsingContext, requestMessage);

        requestMessage.Headers.GetValues("X-Custom-Header").Should().ContainSingle("CustomValue");
        requestMessage.Headers.CacheControl.Should().NotBeNull();
        requestMessage.Headers.CacheControl!.NoCache.Should().BeTrue();
    }

    [Fact]
    public void SetSpecialHeadersCorrectly()
    {
        var parsingContext = new HttpParsingContext(_defaultHeaders);
        parsingContext.AddHeader("Content-Type", "application/json");
        parsingContext.AddHeader("Authorization", "Bearer my-token");

        var requestMessage = new HttpRequestMessage
        {
            Content = new StringContent("")
        };

        _headersHandler.SetHeaders(parsingContext, requestMessage);

        requestMessage.Content.Headers.ContentType.Should().NotBeNull();
        requestMessage.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        requestMessage.Headers.Authorization.Should().NotBeNull();
        requestMessage.Headers.Authorization!.Scheme.Should().Be("Bearer");
        requestMessage.Headers.Authorization.Parameter.Should().Be("my-token");
    }

    [Fact]
    public void ReturnNormalHeaderCorrectly()
    {
        var requestMessage = new HttpRequestMessage();
        requestMessage.Headers.TryAddWithoutValidation("X-Custom-Header", "CustomValue");

        var headerValue = _headersHandler.GetHeader("X-Custom-Header", requestMessage);

        headerValue.Should().Be("CustomValue");
    }

    [Fact]
    public void ReturnSpecialHeaderCorrectly()
    {
        var requestMessage = new HttpRequestMessage
        {
            Content = new StringContent("")
        };
        requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var headerValue = _headersHandler.GetHeader("Content-Type", requestMessage);

        headerValue.Should().Be("application/json");
    }

    [Fact]
    public void ReturnDefaultValueIfHeaderWasNotFound()
    {
        var requestMessage = new HttpRequestMessage();

        var headerValue = _headersHandler.GetHeader("Non-Existent-Header", requestMessage, "DefaultValue");

        headerValue.Should().Be("DefaultValue");
    }

    [Fact]
    public void ThrowExceptionForInvalidNormalHeader()
    {
        var parsingContext = new HttpParsingContext(_defaultHeaders);
        parsingContext.AddHeader("Invalid-Header", "\u0001");

        var requestMessage = new HttpRequestMessage();

        _headersHandler.Invoking(hh => hh.SetHeaders(parsingContext, requestMessage)).Should()
            .Throw<SyntaxErrorException>();
    }

    [Fact]
    public void ThrowExceptionForMissingContentForContentSpecificHeader()
    {
        var parsingContext = new HttpParsingContext(_defaultHeaders);
        parsingContext.AddHeader("Content-Type", "application/json");

        var requestMessage = new HttpRequestMessage();

        _headersHandler.Invoking(hh => hh.SetHeaders(parsingContext, requestMessage)).Should()
           .Throw<InvalidOperationException>()
           .WithMessage("Unable to resolve 'Content-Type' header with the value 'application/json'.");
    }

    [Fact]
    public void GetUserDefinedHeaderCorrectly()
    {
        var responseMessage = new HttpResponseMessage();
        responseMessage.Headers.TryAddWithoutValidation("X-Response-Header", "ResponseValue");

        var headerValue = _headersHandler.GetHeader("X-Response-Header", responseMessage);

        headerValue.Should().Be("ResponseValue");
    }
}
