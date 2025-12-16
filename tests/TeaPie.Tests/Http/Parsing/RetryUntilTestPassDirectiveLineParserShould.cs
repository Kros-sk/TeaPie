using System.Net.Http.Headers;
using TeaPie.Http.Parsing;
using TeaPie.Http.Retrying;
using static Xunit.Assert;

namespace TeaPie.Tests.Http.Parsing;

public class RetryUntilTestPassDirectiveLineParserShould
{
    private readonly HttpRequestHeaders _headers = new HttpClient().DefaultRequestHeaders;
    private readonly RetryUntilTestPassDirectiveLineParser _parser = new();

    [Fact]
    public void ParseRetryUntilStatusCodesDirective()
    {
        const string testName = "TestName";
        const string line = $"## RETRY-UNTIL-TEST-PASS: {testName}";
        HttpParsingContext context = new(_headers);

        True(_parser.CanParse(line, context));
        _parser.Parse(line, context);

        Equal(testName, context.RetryUntilTestPassTestName);
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
