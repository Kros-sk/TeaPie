using FluentAssertions;
using TeaPie.Http.Headers;

namespace TeaPie.Tests.Http.Headers;

public class HostHeaderHandlerShould
{
    private readonly HostHeaderHandler _handler = new();

    [Fact]
    public void SetHeader_SetsHost()
    {
        var request = new HttpRequestMessage();

        _handler.SetHeader("example.com", request);

        request.Headers.Host.Should().Be("example.com");
    }

    [Fact]
    public void GetHeader_ReturnsHost()
    {
        var request = new HttpRequestMessage();
        _handler.SetHeader("example.com", request);

        _handler.GetHeader(request).Should().Be("example.com");
    }

    [Fact]
    public void GetHeader_ReturnsEmpty_WhenNotSet()
    {
        var request = new HttpRequestMessage();

        _handler.GetHeader(request).Should().BeEmpty();
    }

    [Fact]
    public void GetHeader_FromResponse_ReturnsEmpty()
    {
        var response = new HttpResponseMessage();

        _handler.GetHeader(response).Should().BeEmpty();
    }
}
