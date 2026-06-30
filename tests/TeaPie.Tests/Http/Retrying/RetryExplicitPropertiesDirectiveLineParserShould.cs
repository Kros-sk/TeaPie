using FluentAssertions;
using Polly;
using TeaPie.Http.Parsing;
using TeaPie.Http.Retrying;

namespace TeaPie.Tests.Http.Retrying;

public class RetryExplicitPropertiesDirectiveLineParserShould
{
    private readonly RetryExplicitPropertiesDirectiveLineParser _parser = new();

    private static HttpParsingContext CreateContext()
    {
        using var msg = new HttpRequestMessage();
        return new HttpParsingContext(msg.Headers);
    }

    [Fact]
    public void ReturnTrueForMaxAttemptsDirective()
    {
        var context = CreateContext();

        _parser.CanParse("## RETRY-MAX-ATTEMPTS: 5", context).Should().BeTrue();
    }

    [Fact]
    public void ReturnTrueForBackoffTypeDirective()
    {
        var context = CreateContext();

        _parser.CanParse("## RETRY-BACKOFF-TYPE: Linear", context).Should().BeTrue();
    }

    [Fact]
    public void ReturnFalseForUnrelatedDirective()
    {
        var context = CreateContext();

        _parser.CanParse("## UNRELATED-DIRECTIVE: value", context).Should().BeFalse();
    }

    [Fact]
    public void SetMaxRetryAttemptsFromMaxAttemptsDirective()
    {
        var context = CreateContext();

        _parser.Parse("## RETRY-MAX-ATTEMPTS: 5", context);

        context.ExplicitRetryStrategy.Should().NotBeNull();
        context.ExplicitRetryStrategy!.MaxRetryAttempts.Should().Be(5);
    }

    [Fact]
    public void SetBackoffTypeFromBackoffTypeDirective()
    {
        var context = CreateContext();

        _parser.Parse("## RETRY-BACKOFF-TYPE: Linear", context);

        context.ExplicitRetryStrategy.Should().NotBeNull();
        context.ExplicitRetryStrategy!.BackoffType.Should().Be(DelayBackoffType.Linear);
    }

    [Fact]
    public void ThrowForUnparseableLine()
    {
        var context = CreateContext();

        var act = () => _parser.Parse("## INVALID-LINE: something", context);

        act.Should().Throw<InvalidOperationException>();
    }
}
