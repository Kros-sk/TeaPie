using System.Net;
using System.Net.Http.Headers;
using TeaPie.Http.Parsing;
using static Xunit.Assert;

namespace TeaPie.Tests.Http.Parsing;
public class RetryUntilStatusCodesLineParserShould
{
    private readonly HttpRequestHeaders _headers = new HttpClient().DefaultRequestHeaders;
    private readonly RetryUntilStatusCodesLineParser _parser = new();

    [Fact]
    public void ParseRetryUntilStatusCodesDirective()
    {
        const string line = "## RETRY-UNTIL-STATUS: [400, 500]";
        HttpParsingContext context = new(_headers);

        True(_parser.CanParse(line, context));
        _parser.Parse(line, context);

        var expectedCodes = new List<HttpStatusCode> { HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError };
        Equal(expectedCodes, context.RetryUntilStatusCodes);
    }

    [Fact]
    public void ThrowExceptionOnInvalidRetryUntilStatusCodesDirective()
    {
        const string line = "## RETRY-UNTIL-STATUS: INVALID";
        HttpParsingContext context = new(_headers);

        False(_parser.CanParse(line, context));
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
