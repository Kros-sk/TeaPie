using System.Net;
using TeaPie.Http;
using TeaPie.Http.Parsing;
using static Xunit.Assert;

namespace TeaPie.Tests.Http.Parsing;
public class RetryUntilStatusCodesLineParserShould
{
    private readonly HttpParsingContext _context = new(new HttpClient().DefaultRequestHeaders);
    private readonly RetryUntilStatusCodesLineParser _parser = new();

    [Fact]
    public void ParseRetryUntilStatusCodesDirective()
    {
        const string line = "## RETRY-UNTIL-STATUS: [400, 500]";

        True(_parser.CanParse(line, _context));
        _parser.Parse(line, _context);

        var expectedCodes = new List<HttpStatusCode> { HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError };
        Equal(expectedCodes, _context.RetryUntilStatusCodes);
    }

    [Fact]
    public void ThrowExceptionOnInvalidRetryUntilStatusCodesDirective()
    {
        const string line = "## RETRY-UNTIL-STATUS: INVALID";

        False(_parser.CanParse(line, _context));
    }
}
