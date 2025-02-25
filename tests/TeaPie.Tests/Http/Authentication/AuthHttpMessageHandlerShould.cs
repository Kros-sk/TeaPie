using NSubstitute;
using TeaPie.Http.Auth;

namespace TeaPie.Tests.Http.Authentication;

public class AuthHttpMessageHandlerShould
{
    [Fact]
    public async Task AuthenticateRequestWithProvidedAuthProvider()
    {
        var accessor = Substitute.For<IAuthProviderAccessor>();
        var mockAuthProvider = Substitute.For<IAuthProvider>();
        accessor.CurrentProvider = mockAuthProvider;

        var handler = new AuthHttpMessageHandler(accessor)
        {
            InnerHandler = new HttpClientHandler()
        };

        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

        await invoker.SendAsync(request, CancellationToken.None);

        await mockAuthProvider.Received(1).Authenticate(request, CancellationToken.None);
    }
}
