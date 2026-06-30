using FluentAssertions;
using TeaPie.Http.Headers;

namespace TeaPie.Tests.Http.Headers;

public class AuthorizationHeaderHandlerShould
{
    private readonly AuthorizationHeaderHandler _handler = new();

    [Fact]
    public void SetHeader_WithBearerToken_SetsAuthorizationCorrectly()
    {
        var request = new HttpRequestMessage();

        _handler.SetHeader("Bearer token123", request);

        request.Headers.Authorization.Should().NotBeNull();
        request.Headers.Authorization!.Scheme.Should().Be("Bearer");
        request.Headers.Authorization.Parameter.Should().Be("token123");
    }

    [Fact]
    public void SetHeader_WithSingleWord_Throws()
    {
        var request = new HttpRequestMessage();

        var act = () => _handler.SetHeader("BearerOnly", request);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetHeader_ReturnsAuthorizationString()
    {
        var request = new HttpRequestMessage();
        _handler.SetHeader("Bearer token123", request);

        _handler.GetHeader(request).Should().Be("Bearer token123");
    }

    [Fact]
    public void GetHeader_ReturnsEmpty_WhenNotSet()
    {
        var request = new HttpRequestMessage();

        _handler.GetHeader(request).Should().BeEmpty();
    }

    [Fact]
    public void HeaderName_IsAuthorization() => _handler.HeaderName.Should().Be("Authorization");
}
