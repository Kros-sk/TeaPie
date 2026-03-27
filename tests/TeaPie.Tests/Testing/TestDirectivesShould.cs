using System.Text.RegularExpressions;
using FluentAssertions;
using TeaPie.Testing;

namespace TeaPie.Tests.Testing;

public class TestDirectivesShould
{
    [Fact]
    public void TestDirectivePrefix_IsTEST()
    {
        TestDirectives.TestDirectivePrefix.Should().Be("TEST-");
    }

    [Fact]
    public void TestExpectStatusCodesDirectiveFullName_IsTEST_EXPECT_STATUS()
    {
        TestDirectives.TestExpectStatusCodesDirectiveFullName.Should().Be("TEST-EXPECT-STATUS");
    }

    [Fact]
    public void TestHasBodyDirectiveFullName_IsTEST_HAS_BODY()
    {
        TestDirectives.TestHasBodyDirectiveFullName.Should().Be("TEST-HAS-BODY");
    }

    [Fact]
    public void TestHasHeaderDirectiveFullName_IsTEST_HAS_HEADER()
    {
        TestDirectives.TestHasHeaderDirectiveFullName.Should().Be("TEST-HAS-HEADER");
    }

    [Fact]
    public void TestExpectStatusCodesPattern_MatchesSampleDirectiveLine()
    {
        Regex.IsMatch("## TEST-EXPECT-STATUS: [200, 201]", TestDirectives.TestExpectStatusCodesDirectivePattern)
            .Should().BeTrue();
    }

    [Fact]
    public void TestHasBodyPattern_MatchesSampleDirectiveLine()
    {
        Regex.IsMatch("## TEST-HAS-BODY: true", TestDirectives.TestHasBodyDirectivePattern)
            .Should().BeTrue();
    }

    [Fact]
    public void TestHasHeaderPattern_MatchesSampleDirectiveLine()
    {
        Regex.IsMatch("## TEST-HAS-HEADER: Content-Type", TestDirectives.TestHasHeaderDirectivePattern)
            .Should().BeTrue();
    }
}
