using Polly;
using TeaPie.Http.Parsing;
using static Xunit.Assert;

namespace TeaPie.Tests.Http.Parsing;

public class RetryExplicitPropertiesDirectiveLineParserShould
{
    private readonly HttpParsingContext _context = new(new HttpClient().DefaultRequestHeaders);
    private readonly RetryExplicitPropertiesDirectiveLineParser _parser = new();

    [Fact]
    public void ParseRetryMaxAttemptsDirective()
    {
        const string line = "## RETRY-MAX-ATTEMPTS: 5";

        True(_parser.CanParse(line, _context));
        _parser.Parse(line, _context);

        NotNull(_context.ExplicitRetryStrategy);
        Equal(5, _context.ExplicitRetryStrategy.MaxRetryAttempts);
    }

    [Fact]
    public void ParseRetryBackoffTypeDirective()
    {
        const string line = "## RETRY-BACKOFF-TYPE: Exponential";

        True(_parser.CanParse(line, _context));
        _parser.Parse(line, _context);

        NotNull(_context.ExplicitRetryStrategy);
        Equal(DelayBackoffType.Exponential, _context.ExplicitRetryStrategy.BackoffType);
    }

    [Fact]
    public void ParseRetryMaxDelayDirective()
    {
        const string line = "## RETRY-MAX-DELAY: 00:00:03.500";

        True(_parser.CanParse(line, _context));
        _parser.Parse(line, _context);

        NotNull(_context.ExplicitRetryStrategy);
        Equal(TimeSpan.FromMilliseconds(3500), _context.ExplicitRetryStrategy.MaxDelay);
    }
}
