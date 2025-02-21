using System.Net.Http.Headers;
using TeaPie.Http.Parsing;
using static Xunit.Assert;

namespace TeaPie.Tests.Http.Parsing;

public class AuthProviderDirectiveLineParserShould
{
    private readonly HttpRequestHeaders _headers = new HttpClient().DefaultRequestHeaders;
    private readonly AuthProviderDirectiveLineParser _parser = new();

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
    public void NotParseOtherDirective()
    {
        const string line = "## ANOTHER-DIRECTIVE: OAuth2";
        var context = new HttpParsingContext(_headers);

        var canParse = _parser.CanParse(line, context);

        False(canParse);
        Throws<InvalidOperationException>(() => _parser.Parse(line, context));
    }
}
