using System.Text.RegularExpressions;
using FluentAssertions;
using TeaPie.Http.Parsing;

namespace TeaPie.Tests.Http.Parsing;

public class BaseDirectivePatternBuilderShould
{
    [Fact]
    public void Build_WithNoParams_ProducesPatternMatchingDirective()
    {
        var pattern = new BaseDirectivePatternBuilder("MY-DIRECTIVE").Build();
        Regex.IsMatch("## MY-DIRECTIVE", pattern).Should().BeTrue();
    }

    [Fact]
    public void Build_WithStringParameter_ProducesPatternWithStringCapture()
    {
        var pattern = new BaseDirectivePatternBuilder("MY-DIRECTIVE")
            .AddStringParameter("Value")
            .Build();

        Regex.IsMatch("## MY-DIRECTIVE: someValue", pattern).Should().BeTrue();
    }

    [Fact]
    public void Build_WithBooleanParameter_ProducesPatternWithBoolCapture()
    {
        var pattern = new BaseDirectivePatternBuilder("MY-DIRECTIVE")
            .AddBooleanParameter("Flag")
            .Build();

        Regex.IsMatch("## MY-DIRECTIVE: true", pattern).Should().BeTrue();
    }

    [Fact]
    public void Build_WithNumberParameter_ProducesPatternWithNumberCapture()
    {
        var pattern = new BaseDirectivePatternBuilder("MY-DIRECTIVE")
            .AddNumberParameter("Count")
            .Build();

        Regex.IsMatch("## MY-DIRECTIVE: 42", pattern).Should().BeTrue();
    }

    [Fact]
    public void Build_WithPrefix_AddsPrefixToDirectiveName()
    {
        var pattern = new BaseDirectivePatternBuilder("NAME", "PRE-").Build();
        Regex.IsMatch("## PRE-NAME", pattern).Should().BeTrue();
    }

    [Fact]
    public void AddParameter_WithNullName_AutoGeneratesName()
    {
        var pattern = new BaseDirectivePatternBuilder("MY-DIRECTIVE")
            .AddStringParameter()
            .Build();

        pattern.Should().Contain("Parameter1");
    }

    [Fact]
    public void MultipleParameters_JoinedWithSeparator()
    {
        var pattern = new BaseDirectivePatternBuilder("MY-DIRECTIVE")
            .AddStringParameter("P1")
            .AddStringParameter("P2")
            .Build();

        Regex.IsMatch("## MY-DIRECTIVE: val1; val2", pattern).Should().BeTrue();
    }

    [Fact]
    public void Verify_PatternMatches_ActualDirectiveLine()
    {
        var pattern = new BaseDirectivePatternBuilder("AUTH-PROVIDER")
            .AddStringParameter("Provider")
            .Build();

        Regex.IsMatch("## AUTH-PROVIDER: OAuth2", pattern).Should().BeTrue();
    }
}
