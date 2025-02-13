using System.Net;
using TeaPie.Http.Parsing;
using static Xunit.Assert;

namespace TeaPie.Tests.Http.Parsing;

public class RetryDirectivesLineParserShould
{
    private readonly HttpParsingContext _context = new(new HttpClient().DefaultRequestHeaders);
    private readonly RetryDirectivesLineParser _parser = new();

    [Fact]
    public void ParseRetryStrategyDirective()
    {
        const string line = "## RETRY-STRATEGY: MyCustomStrategy";

        True(_parser.CanParse(line, _context));
        _parser.Parse(line, _context);

        Equal("MyCustomStrategy", _context.RetryStrategyName);
    }

    [Fact]
    public void ParseRetryUntilStatusCodesDirective()
    {
        const string line = "## RETRY-UNTIL-STATUS: [500, 502, 503]";

        True(_parser.CanParse(line, _context));
        _parser.Parse(line, _context);

        var expectedCodes = new List<HttpStatusCode>
        {
            HttpStatusCode.InternalServerError,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable
        };

        Equal(expectedCodes, _context.RetryUntilStatusCodes);
    }

    [Fact]
    public void ParseRetryExplicitPropertiesDirective()
    {
        const string line = "## RETRY-MAX-ATTEMPTS: 3";

        True(_parser.CanParse(line, _context));
        _parser.Parse(line, _context);

        NotNull(_context.ExplicitRetryStrategy);
        Equal(3, _context.ExplicitRetryStrategy.MaxRetryAttempts);
    }
}
