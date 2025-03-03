using System.Net.Http.Headers;
using TeaPie.Http.Parsing;
using TeaPie.Testing;
using static Xunit.Assert;

namespace TeaPie.Tests.Http.Parsing;

public class TestDirectivesLineParserShould
{
    private readonly HttpRequestHeaders _headers = new HttpClient().DefaultRequestHeaders;
    private readonly TestDirectivesLineParser _parser = new();

    [Fact]
    public void ParseTestExpectStatusCodesDirective()
    {
        const string line = "## TEST-EXPECT-STATUS: [500, 501]";
        HttpParsingContext context = new(_headers);

        True(_parser.CanParse(line, context));
        _parser.Parse(line, context);

        NotEmpty(context.Tests);
        var testDesc = context.Tests[0];
        Equal("TEST-EXPECT-STATUS", testDesc.Directive);
        Equal("[500, 501]", testDesc.Parameters[TestDirectives.TestExpectStatusCodesParameterName]);
    }

    [Fact]
    public void ParseTestHasBodyWithArgumentDirective()
    {
        const string line = "## TEST-HAS-BODY: false";
        HttpParsingContext context = new(_headers);

        True(_parser.CanParse(line, context));
        _parser.Parse(line, context);

        NotEmpty(context.Tests);
        var testDesc = context.Tests[0];
        Equal("TEST-HAS-BODY", testDesc.Directive);
        False(bool.Parse(testDesc.Parameters[TestDirectives.TestHasBodyDirectiveParameterName]));
    }

    [Fact]
    public void ParseTestHasBodyWithoutArgumentDirective()
    {
        const string line = "## TEST-HAS-BODY";
        HttpParsingContext context = new(_headers);

        True(_parser.CanParse(line, context));
        _parser.Parse(line, context);

        NotEmpty(context.Tests);
        var testDesc = context.Tests[0];
        Equal("TEST-HAS-BODY", testDesc.Directive);
        False(testDesc.Parameters.ContainsKey(TestDirectives.TestHasBodyDirectiveParameterName));
    }

    [Fact]
    public void ParseTestHasHeaderDirective()
    {
        const string headerName = "Content-Type";
        var line = $"## TEST-HAS-HEADER: {headerName}";
        HttpParsingContext context = new(_headers);

        True(_parser.CanParse(line, context));
        _parser.Parse(line, context);

        NotEmpty(context.Tests);
        var testDesc = context.Tests[0];
        Equal("TEST-HAS-HEADER", testDesc.Directive);
        Equal(headerName, testDesc.Parameters[TestDirectives.TestHasHeaderDirectiveParameterName]);
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
