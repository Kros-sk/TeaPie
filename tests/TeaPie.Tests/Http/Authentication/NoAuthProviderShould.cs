using FluentAssertions;
using TeaPie.Http.Auth;

namespace TeaPie.Tests.Http.Authentication;

public class NoAuthProviderShould
{
    [Fact]
    public async Task Authenticate_CompletesWithoutThrowing()
    {
        var provider = new NoAuthProvider();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        var act = () => provider.Authenticate(request, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Authenticate_DoesNotModifyRequest()
    {
        var provider = new NoAuthProvider();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        request.Headers.Add("X-Custom", "value");

        await provider.Authenticate(request, CancellationToken.None);

        request.Headers.GetValues("X-Custom").Should().ContainSingle().Which.Should().Be("value");
        request.Headers.Authorization.Should().BeNull();
    }
}
