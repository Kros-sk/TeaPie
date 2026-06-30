using FluentAssertions;
using System.Net.Http.Headers;
using TeaPie.Http.Headers;

namespace TeaPie.Tests.Http.Headers;

public class ContentTypeHeaderHandlerShould
{
    private readonly ContentTypeHeaderHandler _handler = new();

    [Fact]
    public void SetHeader_SetsContentType()
    {
        var request = new HttpRequestMessage { Content = new StringContent("body") };

        _handler.SetHeader("application/json", request);

        request.Content.Headers.ContentType.Should().NotBeNull();
        request.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
    }

    [Fact]
    public void GetHeader_ReturnsContentType()
    {
        var request = new HttpRequestMessage { Content = new StringContent("body") };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        _handler.GetHeader(request).Should().Be("application/json");
    }

    [Fact]
    public void SetHeader_Throws_WhenContentIsNull()
    {
        var request = new HttpRequestMessage();

        var act = () => _handler.SetHeader("application/json", request);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void HeaderName_IsContentType() => _handler.HeaderName.Should().Be("Content-Type");
}
