using FluentAssertions;
using TeaPie.Http.Parsing;

namespace TeaPie.Tests.Http.Parsing;

public class EmptyLineParserShould
{
    private readonly EmptyLineParser _parser = new();

    private static HttpParsingContext CreateContext()
    {
        using var requestMessage = new HttpRequestMessage();
        return new HttpParsingContext(requestMessage.Headers);
    }

    [Fact]
    public void ReturnTrueForEmptyString()
    {
        var context = CreateContext();

        _parser.CanParse("", context).Should().BeTrue();
    }

    [Fact]
    public void ReturnTrueForWhitespaceOnly()
    {
        var context = CreateContext();

        _parser.CanParse("   ", context).Should().BeTrue();
    }

    [Fact]
    public void ReturnFalseForNonEmptyLine()
    {
        var context = CreateContext();

        _parser.CanParse("GET https://example.com", context).Should().BeFalse();
    }

    [Fact]
    public void SetIsBodyWhenMethodAndUriResolved()
    {
        var context = CreateContext();
        context.IsMethodAndUriResolved = true;

        _parser.Parse("", context);

        context.IsBody.Should().BeTrue();
    }

    [Fact]
    public void NotSetIsBodyWhenMethodAndUriNotResolved()
    {
        var context = CreateContext();

        _parser.Parse("", context);

        context.IsBody.Should().BeFalse();
    }
}
