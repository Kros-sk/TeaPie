using System.Text.RegularExpressions;
using FluentAssertions;
using TeaPie.Http.Retrying;

namespace TeaPie.Tests.Http.Retrying;

public class RetryingDirectivesShould
{
    [Fact]
    public void RetryDirectivePrefix_IsRETRY()
    {
        RetryingDirectives.RetryDirectivePrefix.Should().Be("RETRY-");
    }

    [Fact]
    public void RetryStrategyDirectiveFullName_IsRETRY_STRATEGY()
    {
        RetryingDirectives.RetryStrategyDirectiveFullName.Should().Be("RETRY-STRATEGY");
    }

    [Fact]
    public void RetryUntilStatusCodesDirectiveFullName_IsRETRY_UNTIL_STATUS()
    {
        RetryingDirectives.RetryUntilStatusCodesDirectiveFullName.Should().Be("RETRY-UNTIL-STATUS");
    }

    [Fact]
    public void RetryMaxAttemptsDirectiveFullName_IsRETRY_MAX_ATTEMPTS()
    {
        RetryingDirectives.RetryMaxAttemptsDirectiveFullName.Should().Be("RETRY-MAX-ATTEMPTS");
    }

    [Fact]
    public void RetryBackoffTypeDirectiveFullName_IsRETRY_BACKOFF_TYPE()
    {
        RetryingDirectives.RetryBackoffTypeDirectiveFullName.Should().Be("RETRY-BACKOFF-TYPE");
    }

    [Fact]
    public void RetryMaxDelayDirectiveFullName_IsRETRY_MAX_DELAY()
    {
        RetryingDirectives.RetryMaxDelayDirectiveFullName.Should().Be("RETRY-MAX-DELAY");
    }

    [Fact]
    public void RetryUntilTestPassDirectiveFullName_IsRETRY_UNTIL_TEST_PASS()
    {
        RetryingDirectives.RetryUntilTestPassDirectiveFullName.Should().Be("RETRY-UNTIL-TEST-PASS");
    }

    [Theory]
    [InlineData("## RETRY-STRATEGY: MyStrategy")]
    [InlineData("## RETRY-STRATEGY: Default")]
    public void RetryStrategyPattern_MatchesSampleDirectiveLine(string line)
    {
        Regex.IsMatch(line, RetryingDirectives.RetryStrategySelectorDirectivePattern)
            .Should().BeTrue();
    }

    [Theory]
    [InlineData("## RETRY-MAX-ATTEMPTS: 5")]
    [InlineData("## RETRY-MAX-ATTEMPTS: 10")]
    public void RetryMaxAttemptsPattern_MatchesSampleDirectiveLine(string line)
    {
        Regex.IsMatch(line, RetryingDirectives.RetryMaxAttemptsDirectivePattern)
            .Should().BeTrue();
    }

    [Theory]
    [InlineData("## RETRY-UNTIL-STATUS: [200, 201]")]
    [InlineData("## RETRY-UNTIL-STATUS: [500]")]
    public void RetryUntilStatusCodesPattern_MatchesSampleDirectiveLine(string line)
    {
        Regex.IsMatch(line, RetryingDirectives.RetryUntilStatusCodesDirectivePattern)
            .Should().BeTrue();
    }
}
