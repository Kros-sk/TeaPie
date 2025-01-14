using FluentAssertions;
using System.Net;
using TeaPie.Http;

namespace TeaPie.Tests.Http;

public class HttpMessagesExtensionsShould
{
    private static HttpRequestMessage CreateRequestWithContent(string content)
        => new() { Content = new StringContent(content) };

    private static HttpResponseMessage CreateResponseWithContent(string content)
        => new() { Content = new StringContent(content) };

    [Fact]
    public void ReturnContentWhenGetBodyCalledOnRequestWithContent()
    {
        var request = CreateRequestWithContent("Request Body Content");

        var result = request.GetBody();

        result.Should().Be("Request Body Content");
    }

    [Fact]
    public void ReturnEmptyStringWhenGetBodyCalledOnRequestWithoutContent()
    {
        var request = new HttpRequestMessage();

        var result = request.GetBody();

        result.Should().BeEmpty();
    }

    [Fact]
    public void ReturnContentWhenGetBodyCalledOnResponseWithContent()
    {
        var response = CreateResponseWithContent("Response Body Content");

        var result = response.GetBody();

        result.Should().Be("Response Body Content");
    }

    [Fact]
    public void ReturnEmptyStringWhenGetBodyCalledOnResponseWithoutContent()
    {
        var response = new HttpResponseMessage();

        var result = response.GetBody();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ReturnContentWhenGetBodyAsyncCalledOnRequestWithContent()
    {
        var request = CreateRequestWithContent("Async Request Body Content");

        var result = await request.GetBodyAsync();

        result.Should().Be("Async Request Body Content");
    }

    [Fact]
    public async Task ReturnEmptyStringWhenGetBodyAsyncCalledOnRequestWithoutContent()
    {
        var request = new HttpRequestMessage();

        var result = await request.GetBodyAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ReturnContentWhenGetBodyAsyncCalledOnResponseWithContent()
    {
        var response = CreateResponseWithContent("Async Response Body Content");

        var result = await response.GetBodyAsync();

        result.Should().Be("Async Response Body Content");
    }

    [Fact]
    public async Task ReturnEmptyStringWhenGetBodyAsyncCalledOnResponseWithoutContent()
    {
        var response = new HttpResponseMessage();

        var result = await response.GetBodyAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public void ReturnCorrectStatusCodeWhenStatusCodeCalledOnResponse()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        var statusCode = response.StatusCode();

        statusCode.Should().Be(200);
    }
}
