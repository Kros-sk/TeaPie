using FluentAssertions;
using TeaPie.Http.Headers;

namespace TeaPie.Tests.Http.Headers;

public class UserAgentHeaderHandlerShould
{
    private readonly UserAgentHeaderHandler _handler = new();

    [Fact]
    public void SetHeader_AddsUserAgent()
    {
        var request = new HttpRequestMessage();

        _handler.SetHeader("MyApp/1.0", request);

        _handler.GetHeader(request).Should().Contain("MyApp/1.0");
    }

    [Fact]
    public void GetHeader_ReturnsUserAgentString()
    {
        var request = new HttpRequestMessage();
        _handler.SetHeader("MyApp/1.0", request);

        _handler.GetHeader(request).Should().NotBeEmpty();
    }

    [Fact]
    public void GetHeader_FromResponse_ReturnsEmpty()
    {
        var response = new HttpResponseMessage();

        _handler.GetHeader(response).Should().BeEmpty();
    }
}
