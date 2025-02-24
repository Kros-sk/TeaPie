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
        registry.Register(name, mockProvider);

        var result = registry.Get(name);

        True(registry.IsRegistered(name));
        Same(mockProvider, result);
    }

    [Fact]
    public void HaveRegisteredNoAuthProviderByDefault()
    {
        var registry = new AuthProviderRegistry();
        True(registry.IsRegistered(AuthConstants.NoAuthKey));
    }
}
