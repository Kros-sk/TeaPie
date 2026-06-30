using System.Text.RegularExpressions;
using FluentAssertions;
using TeaPie.Http.Auth;

namespace TeaPie.Tests.Http.Authentication;

public class AuthDirectivesShould
{
    [Fact]
    public void AuthDirectivePrefix_IsAUTH()
    {
        AuthDirectives.AuthDirectivePrefix.Should().Be("AUTH-");
    }

    [Fact]
    public void AuthProviderDirectiveName_IsPROVIDER()
    {
        AuthDirectives.AuthProviderDirectiveName.Should().Be("PROVIDER");
    }

    [Fact]
    public void AuthProviderDirectiveFullName_IsAUTH_PROVIDER()
    {
        AuthDirectives.AuthProviderDirectiveFullName.Should().Be("AUTH-PROVIDER");
    }

    [Fact]
    public void AuthProviderDirectivePattern_MatchesSampleDirective()
    {
        Regex.IsMatch("## AUTH-PROVIDER: OAuth2", AuthDirectives.AuthProviderSelectorDirectivePattern)
            .Should().BeTrue();
    }
}
