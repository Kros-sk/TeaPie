using FluentAssertions;
using TeaPie.Http.Parsing;

namespace TeaPie.Tests.Http.Parsing;

public class MethodAndUriParserShould
{
    private readonly MethodAndUriParser _parser = new();

    private static HttpParsingContext CreateContext()
    {
        using var requestMessage = new HttpRequestMessage();
        return new HttpParsingContext(requestMessage.Headers);
    }

    [Fact]
    public void ReturnTrueForValidMethodAndUriLine()
    {
        var context = CreateContext();

        _parser.CanParse("GET https://example.com", context).Should().BeTrue();
    }

    [Fact]
    public void ReturnFalseWhenMethodAndUriAlreadyResolved()
    {
        var context = CreateContext();
        context.IsMethodAndUriResolved = true;

        _parser.CanParse("GET https://example.com", context).Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnFalseForEmptyOrWhitespaceLine(string line)
    {
        var context = CreateContext();

        _parser.CanParse(line, context).Should().BeFalse();
    }

    [Fact]
    public void ReturnFalseForCommentLine()
    {
        var context = CreateContext();

        _parser.CanParse("# this is a comment", context).Should().BeFalse();
    }

    [Fact]
    public void ReturnFalseForAltCommentLine()
    {
        var context = CreateContext();

        _parser.CanParse("// this is a comment", context).Should().BeFalse();
    }

    [Fact]
    public void ReturnFalseWhenIsBodyIsTrue()
    {
        var context = CreateContext();
        context.IsBody = true;

        _parser.CanParse("GET https://example.com", context).Should().BeFalse();
    }

    [Fact]
    public void ParseGetMethodAndUriCorrectly()
    {
        var context = CreateContext();

        _parser.Parse("GET https://example.com/api", context);

        context.Method.Should().Be(HttpMethod.Get);
        context.RequestUri.Should().Be("https://example.com/api");
    }

    [Fact]
    public void ParsePostMethodAndUriCorrectly()
    {
        var context = CreateContext();

        _parser.Parse("POST https://example.com/api/items", context);

        context.Method.Should().Be(HttpMethod.Post);
        context.RequestUri.Should().Be("https://example.com/api/items");
    }

    [Fact]
    public void ThrowForSingleWordLine()
    {
        var context = CreateContext();

        var act = () => _parser.Parse("GETWITHNOURL", context);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ThrowForUnsupportedMethod()
    {
        var context = CreateContext();

        var act = () => _parser.Parse("UNKNOWN https://example.com", context);

        act.Should().Throw<InvalidOperationException>().WithMessage("*UNKNOWN*");
    }

    [Fact]
    public void SetIsMethodAndUriResolvedAfterParsing()
    {
        var context = CreateContext();

        _parser.Parse("GET https://example.com", context);

        context.IsMethodAndUriResolved.Should().BeTrue();
    }
}
