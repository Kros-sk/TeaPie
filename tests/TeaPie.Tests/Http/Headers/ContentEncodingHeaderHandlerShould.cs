using FluentAssertions;
using TeaPie.Http.Headers;

namespace TeaPie.Tests.Http.Headers;

public class ContentEncodingHeaderHandlerShould
{
    private readonly ContentEncodingHeaderHandler _handler = new();

    [Fact]
    public void SetHeader_AddsEncoding()
    {
        var request = new HttpRequestMessage { Content = new StringContent("body") };

        _handler.SetHeader("gzip", request);

        request.Content.Headers.ContentEncoding.Should().Contain("gzip");
    }

    [Fact]
    public void GetHeader_ReturnsEncoding()
    {
        var request = new HttpRequestMessage { Content = new StringContent("body") };
        request.Content.Headers.ContentEncoding.Add("gzip");

        _handler.GetHeader(request).Should().Be("gzip");
    }

    [Fact]
    public void SetHeader_Throws_WhenContentIsNull()
    {
        var request = new HttpRequestMessage();

        var act = () => _handler.SetHeader("gzip", request);

        act.Should().Throw<InvalidOperationException>();
    }
}
