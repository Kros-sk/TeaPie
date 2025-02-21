using NSubstitute;
using TeaPie.Http.Auth;
using static Xunit.Assert;

namespace TeaPie.Tests.Http.Authentication;

public class AuthProviderRegistryShould
{
    [Fact]
    public void RegisterAuthProviderSuccessfully()
    {
        const string name = "OAuth2";
        var registry = new AuthProviderRegistry();
        var mockProvider = Substitute.For<IAuthProvider>();
        registry.RegisterAuthProvider(name, mockProvider);

        var result = registry.GetAuthProvider(name);

        True(registry.IsAuthProviderRegistered(name));
        Same(mockProvider, result);
    }

    [Fact]
    public void HaveRegisteredNoAuthProviderByDefault()
    {
        var registry = new AuthProviderRegistry();
        True(registry.IsAuthProviderRegistered(AuthConstants.NoAuthKey));
    }
}
