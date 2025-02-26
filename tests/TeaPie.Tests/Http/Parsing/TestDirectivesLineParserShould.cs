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
        Equal(PredefinedTestType.ExpectStatusCodes, testDesc.Type);
        Equal([500, 501], (int[])testDesc.Parameters[0]);
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
        Equal(PredefinedTestType.HasBody, testDesc.Type);
        False((bool)testDesc.Parameters[0]);
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
        Equal(PredefinedTestType.HasBody, testDesc.Type);
        True((bool)testDesc.Parameters[0]);
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
        Equal(PredefinedTestType.HasHeader, testDesc.Type);
        Equal(headerName, (string)testDesc.Parameters[0]);
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
