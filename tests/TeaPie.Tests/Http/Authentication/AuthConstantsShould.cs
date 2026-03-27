using FluentAssertions;
using TeaPie.Http.Auth;

namespace TeaPie.Tests.Http.Authentication;

public class AuthConstantsShould
{
    [Fact]
    public void NoAuthKey_IsNone()
    {
        AuthConstants.NoAuthKey.Should().Be("None");
    }

    [Fact]
    public void OAuth2Key_IsOAuth2()
    {
        AuthConstants.OAuth2Key.Should().Be("OAuth2");
    }
}
