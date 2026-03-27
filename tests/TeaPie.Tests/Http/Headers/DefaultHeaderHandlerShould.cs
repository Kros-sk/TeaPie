using FluentAssertions;
using TeaPie.Http.Headers;

namespace TeaPie.Tests.Http.Headers;

public class DefaultHeaderHandlerShould
{
    private readonly DefaultHeaderHandler _handler = new("X-Custom");

    [Fact]
    public void SetHeader_AddsHeaderToRequest()
    {
        var request = new HttpRequestMessage();

        _handler.SetHeader("value1", request);

        request.Headers.GetValues("X-Custom").Should().ContainSingle("value1");
    }

    [Fact]
    public void GetHeader_ReturnsHeaderFromRequest()
    {
        var request = new HttpRequestMessage();
        request.Headers.TryAddWithoutValidation("X-Custom", "value1");

        _handler.GetHeader(request).Should().Be("value1");
    }

    [Fact]
    public void GetHeader_ReturnsEmptyString_WhenHeaderNotSet()
    {
        var request = new HttpRequestMessage();

        _handler.GetHeader(request).Should().BeEmpty();
    }

    [Fact]
    public void GetHeader_FromResponse_Works()
    {
        var response = new HttpResponseMessage();
        response.Headers.TryAddWithoutValidation("X-Custom", "responseValue");

        _handler.GetHeader(response).Should().Be("responseValue");
    }

    [Fact]
    public void HeaderName_PropertyIsSet() => _handler.HeaderName.Should().Be("X-Custom");
}
