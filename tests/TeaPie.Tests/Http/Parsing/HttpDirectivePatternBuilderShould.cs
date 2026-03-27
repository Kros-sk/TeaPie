using System.Text.RegularExpressions;
using FluentAssertions;
using TeaPie.Http.Parsing;

namespace TeaPie.Tests.Http.Parsing;

public class HttpDirectivePatternBuilderShould
{
    [Fact]
    public void Create_ReturnsNewBuilder() =>
        HttpDirectivePatternBuilder.Create("TEST").Should().NotBeNull();

    [Fact]
    public void WithPrefix_SetsPrefix()
    {
        var pattern = HttpDirectivePatternBuilder.Create("NAME")
            .WithPrefix("PRE-")
            .Build();

        Regex.IsMatch("## PRE-NAME", pattern).Should().BeTrue();
    }

    [Fact]
    public void Build_WithPrefixAndParameters_ProducesCorrectPattern()
    {
        var pattern = HttpDirectivePatternBuilder.Create("NAME")
            .WithPrefix("PRE-")
            .AddStringParameter("Param")
            .Build();

        Regex.IsMatch("## PRE-NAME: someValue", pattern).Should().BeTrue();
    }

    [Fact]
    public void Verify_BuiltPattern_MatchesSampleDirectiveLine()
    {
        var pattern = HttpDirectivePatternBuilder.Create("AUTH-PROVIDER")
            .AddStringParameter("Provider")
            .Build();

        Regex.IsMatch("## AUTH-PROVIDER: OAuth2", pattern).Should().BeTrue();
    }
}
