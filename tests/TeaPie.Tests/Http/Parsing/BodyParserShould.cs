using FluentAssertions;
using TeaPie.Http.Parsing;

namespace TeaPie.Tests.Http.Parsing;

public class BodyParserShould
{
    private readonly BodyParser _parser = new();

    private static HttpParsingContext CreateContext()
    {
        using var requestMessage = new HttpRequestMessage();
        return new HttpParsingContext(requestMessage.Headers);
    }

    [Fact]
    public void ReturnTrueWhenIsBodyIsTrue()
    {
        var context = CreateContext();
        context.IsBody = true;

        _parser.CanParse("any line", context).Should().BeTrue();
    }

    [Fact]
    public void ReturnFalseWhenIsBodyIsFalse()
    {
        var context = CreateContext();

        _parser.CanParse("any line", context).Should().BeFalse();
    }

    [Fact]
    public void AppendLineToBodyBuilder()
    {
        var context = CreateContext();
        context.IsBody = true;

        _parser.Parse("line one", context);
        _parser.Parse("line two", context);

        var body = context.BodyBuilder.ToString();
        body.Should().Contain("line one");
        body.Should().Contain("line two");
    }
}
