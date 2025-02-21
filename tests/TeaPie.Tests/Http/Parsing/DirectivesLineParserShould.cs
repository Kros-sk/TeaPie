using System.Net;
using System.Net.Http.Headers;
using TeaPie.Http.Parsing;
using static Xunit.Assert;

namespace TeaPie.Tests.Http.Parsing;

public class DirectivesLineParserShould
{
    private readonly HttpRequestHeaders _headers = new HttpClient().DefaultRequestHeaders;
    private readonly DirectivesLineParser _parser = new();

    [Fact]
    public void ParseRetryStrategyDirective()
    {
        const string line = "## RETRY-STRATEGY: MyCustomStrategy";
        var context = new HttpParsingContext(_headers);

        True(_parser.CanParse(line, context));
        _parser.Parse(line, context);

        Equal("MyCustomStrategy", context.RetryStrategyName);
    }

    [Fact]
    public void ParseRetryUntilStatusCodesDirective()
    {
        const string line = "## RETRY-UNTIL-STATUS: [500, 502, 503]";
        var context = new HttpParsingContext(_headers);

        True(_parser.CanParse(line, context));
        _parser.Parse(line, context);

        var expectedCodes = new List<HttpStatusCode>
        {
            HttpStatusCode.InternalServerError,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable
        };

        Equal(expectedCodes, context.RetryUntilStatusCodes);
    }

    [Fact]
    public void ParseRetryExplicitPropertiesDirective()
    {
        const string line = "## RETRY-MAX-ATTEMPTS: 3";
        var context = new HttpParsingContext(_headers);

        True(_parser.CanParse(line, context));
        _parser.Parse(line, context);

        NotNull(context.ExplicitRetryStrategy);
        Equal(3, context.ExplicitRetryStrategy.MaxRetryAttempts);
    }

    [Fact]
    public void ParseAuthProviderDirectiveCorrectly()
    {
        const string line = "## AUTH-PROVIDER: OAuth2";
        var context = new HttpParsingContext(_headers);

        var canParse = _parser.CanParse(line, context);
        _parser.Parse(line, context);

        True(canParse);
        Equal("OAuth2", context.AuthProviderName);
    }

    [Fact]
    public void NotParseUnsupportedDirective()
    {
        const string line = "## ANOTHER-DIRECTIVE: OAuth2";
        var context = new HttpParsingContext(_headers);

        var canParse = _parser.CanParse(line, context);

        True(_parser.CanParse(line, context));
        Throws<InvalidOperationException>(() => _parser.Parse(line, context));
    }
}
