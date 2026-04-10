using FluentAssertions;
using TeaPie.Http.Headers;

namespace TeaPie.Tests.Http.Headers;

public class ContentDispositionHeaderHandlerShould
{
    private readonly ContentDispositionHeaderHandler _handler = new();

    [Fact]
    public void SetHeader_SetsContentDisposition()
    {
        var request = new HttpRequestMessage { Content = new StringContent("body") };

        _handler.SetHeader("attachment", request);

        request.Content.Headers.ContentDisposition.Should().NotBeNull();
        request.Content.Headers.ContentDisposition!.DispositionType.Should().Be("attachment");
    }

    [Fact]
    public void GetHeader_ReturnsContentDisposition()
    {
        var request = new HttpRequestMessage { Content = new StringContent("body") };
        _handler.SetHeader("attachment", request);

        _handler.GetHeader(request).Should().Contain("attachment");
    }

    [Fact]
    public void SetHeader_Throws_WhenContentIsNull()
    {
        var request = new HttpRequestMessage();

        var act = () => _handler.SetHeader("attachment", request);

        act.Should().Throw<InvalidOperationException>();
    }
}
