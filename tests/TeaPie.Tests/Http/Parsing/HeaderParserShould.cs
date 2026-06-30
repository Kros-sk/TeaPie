using System.Data;
using FluentAssertions;
using TeaPie.Http.Parsing;

namespace TeaPie.Tests.Http.Parsing;

public class HeaderParserShould
{
    private readonly HeaderParser _parser = new();

    private static HttpParsingContext CreateContext()
    {
        using var requestMessage = new HttpRequestMessage();
        return new HttpParsingContext(requestMessage.Headers);
    }

    [Fact]
    public void ReturnTrueForValidHeaderLine()
    {
        var context = CreateContext();

        _parser.CanParse("Content-Type: application/json", context).Should().BeTrue();
    }

    [Fact]
    public void ReturnFalseWhenIsBodyIsTrue()
    {
        var context = CreateContext();
        context.IsBody = true;

        _parser.CanParse("Content-Type: application/json", context).Should().BeFalse();
    }

    [Fact]
    public void ReturnFalseForLineWithoutColon()
    {
        var context = CreateContext();

        _parser.CanParse("NoColonHere", context).Should().BeFalse();
    }

    [Fact]
    public void AddHeaderToContext()
    {
        var context = CreateContext();

        _parser.Parse("X-Custom-Header: my-value", context);

        context.Headers.Should().ContainKey("X-Custom-Header");
        context.Headers["X-Custom-Header"].Should().Be("my-value");
    }

    [Fact]
    public void ThrowForInvalidHeaderName()
    {
        var context = CreateContext();

        var act = () => _parser.Parse("Invalid Header Name: value", context);

        act.Should().Throw<SyntaxErrorException>();
    }
}
