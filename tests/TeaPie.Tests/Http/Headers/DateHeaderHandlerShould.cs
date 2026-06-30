using FluentAssertions;
using TeaPie.Http.Headers;

namespace TeaPie.Tests.Http.Headers;

public class DateHeaderHandlerShould
{
    private readonly DateHeaderHandler _handler = new();

    [Fact]
    public void SetHeader_WithValidDate_SetsHeader()
    {
        var request = new HttpRequestMessage();

        _handler.SetHeader("2024-01-15T10:30:00Z", request);

        request.Headers.Date.Should().NotBeNull();
    }

    [Fact]
    public void SetHeader_WithInvalidDate_Throws()
    {
        var request = new HttpRequestMessage();

        var act = () => _handler.SetHeader("not-a-date", request);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetHeader_ReturnsFormattedDate()
    {
        var request = new HttpRequestMessage();
        _handler.SetHeader("2024-01-15T10:30:00Z", request);

        _handler.GetHeader(request).Should().NotBeEmpty();
    }

    [Fact]
    public void GetHeader_ReturnsEmpty_WhenNotSet()
    {
        var request = new HttpRequestMessage();

        _handler.GetHeader(request).Should().BeEmpty();
    }
}
