using TeaPie.Http;
using TeaPie.Http.Parsing;
using static Xunit.Assert;

namespace TeaPie.Tests.Http.Parsing;

public class RetryStrategyLineParserShould
{
    private readonly HttpParsingContext _context = new(new HttpClient().DefaultRequestHeaders);
    private readonly RetryStrategyLineParser _parser = new();

    [Fact]
    public void ParseRetryStrategyDirective()
    {
        const string line = "## RETRY-STRATEGY: FastRetry";

        True(_parser.CanParse(line, _context));
        _parser.Parse(line, _context);

        Equal("FastRetry", _context.RetryStrategyName);
    }

    [Fact]
    public void ThrowExceptionOnInvalidRetryStrategyDirective()
    {
        const string line = "## INVALID-STRATEGY: SomethingElse";

        False(_parser.CanParse(line, _context));
    }
}
