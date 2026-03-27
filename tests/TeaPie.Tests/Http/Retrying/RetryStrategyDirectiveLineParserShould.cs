using FluentAssertions;
using TeaPie.Http.Parsing;
using TeaPie.Http.Retrying;

namespace TeaPie.Tests.Http.Retrying;

public class RetryStrategyDirectiveLineParserShould
{
    private readonly RetryStrategyDirectiveLineParser _parser = new();

    private static HttpParsingContext CreateContext()
    {
        using var msg = new HttpRequestMessage();
        return new HttpParsingContext(msg.Headers);
    }

    [Fact]
    public void ReturnTrueForMatchingRetryStrategyDirective()
    {
        var context = CreateContext();

        _parser.CanParse("## RETRY-STRATEGY: MyStrategy", context).Should().BeTrue();
    }

    [Fact]
    public void ReturnFalseForNonMatchingLine()
    {
        var context = CreateContext();

        _parser.CanParse("## SOMETHING-ELSE: value", context).Should().BeFalse();
    }

    [Fact]
    public void ExtractStrategyNameIntoContext()
    {
        var context = CreateContext();

        _parser.Parse("## RETRY-STRATEGY: MyStrategy", context);

        context.RetryStrategyName.Should().Be("MyStrategy");
    }
}
