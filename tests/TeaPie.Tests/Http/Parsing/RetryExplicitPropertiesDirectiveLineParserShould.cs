using Polly;
using System.Net.Http.Headers;
using TeaPie.Http.Parsing;
using TeaPie.Http.Retrying;
using static Xunit.Assert;

namespace TeaPie.Tests.Http.Parsing;

public class RetryExplicitPropertiesDirectiveLineParserShould
{
    private readonly HttpRequestHeaders _headers = new HttpClient().DefaultRequestHeaders;
    private readonly RetryExplicitPropertiesDirectiveLineParser _parser = new();

    [Fact]
    public void ParseRetryMaxAttemptsDirective()
    {
        const string line = "## RETRY-MAX-ATTEMPTS: 5";
        HttpParsingContext context = new(_headers);

        True(_parser.CanParse(line, context));
        _parser.Parse(line, context);

        NotNull(context.ExplicitRetryStrategy);
        Equal(5, context.ExplicitRetryStrategy.MaxRetryAttempts);
    }

    [Fact]
    public void ParseRetryBackoffTypeDirective()
    {
        const string line = "## RETRY-BACKOFF-TYPE: Exponential";
        HttpParsingContext context = new(_headers);

        True(_parser.CanParse(line, context));
        _parser.Parse(line, context);

        NotNull(context.ExplicitRetryStrategy);
        Equal(DelayBackoffType.Exponential, context.ExplicitRetryStrategy.BackoffType);
    }

    [Fact]
    public void ParseRetryMaxDelayDirective()
    {
        const string line = "## RETRY-MAX-DELAY: 00:00:03.500";
        HttpParsingContext context = new(_headers);

        True(_parser.CanParse(line, context));
        _parser.Parse(line, context);

        NotNull(context.ExplicitRetryStrategy);
        Equal(TimeSpan.FromMilliseconds(3500), context.ExplicitRetryStrategy.MaxDelay);
    }

    [Fact]
    public void NotParseOtherDirective()
    {
        const string line = "## ANOTHER-DIRECTIVE: OAuth2";
        var context = new HttpParsingContext(_headers);

        var canParse = _parser.CanParse(line, context);

        False(canParse);
        Throws<InvalidOperationException>(() => _parser.Parse(line, context));
    }
}
