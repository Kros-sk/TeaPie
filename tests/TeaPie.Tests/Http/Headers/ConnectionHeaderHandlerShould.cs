using FluentAssertions;
using TeaPie.Http.Headers;

namespace TeaPie.Tests.Http.Headers;

public class ConnectionHeaderHandlerShould
{
    private readonly ConnectionHeaderHandler _handler = new();

    [Fact]
    public void SetHeader_WithKeepAlive_SetsHeader()
    {
        var request = new HttpRequestMessage();

        _handler.SetHeader("keep-alive", request);

        request.Headers.TryGetValues("Connection", out var values).Should().BeTrue();
        values.Should().Contain("keep-alive");
    }

    [Fact]
    public void SetHeader_WithClose_SetsConnectionClose()
    {
        var request = new HttpRequestMessage();

        _handler.SetHeader("close", request);

        request.Headers.ConnectionClose.Should().BeTrue();
    }

    [Fact]
    public void SetHeader_WithKeepAliveAndClose_HandlesBoth()
    {
        var request = new HttpRequestMessage();

        _handler.SetHeader("keep-alive, close", request);

        request.Headers.ConnectionClose.Should().BeTrue();
        _handler.GetHeader(request).Should().Contain("keep-alive");
        _handler.GetHeader(request).Should().Contain("close");
    }

    [Fact]
    public void GetHeader_IncludesClose_WhenConnectionCloseSet()
    {
        var request = new HttpRequestMessage();
        request.Headers.ConnectionClose = true;

        _handler.GetHeader(request).Should().Contain("close");
    }
}
