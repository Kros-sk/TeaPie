using FluentAssertions;
using TeaPie.Http.Parsing;

namespace TeaPie.Tests.Http.Parsing;

public class CommentLineParserShould
{
    private readonly CommentLineParser _parser = new();

    private static HttpParsingContext CreateContext()
    {
        using var requestMessage = new HttpRequestMessage();
        return new HttpParsingContext(requestMessage.Headers);
    }

    [Fact]
    public void ReturnTrueForHashComment()
    {
        var context = CreateContext();

        _parser.CanParse("# this is a comment", context).Should().BeTrue();
    }

    [Fact]
    public void ReturnTrueForSlashComment()
    {
        var context = CreateContext();

        _parser.CanParse("// this is a comment", context).Should().BeTrue();
    }

    [Fact]
    public void ReturnFalseForNonCommentLine()
    {
        var context = CreateContext();

        _parser.CanParse("GET https://example.com", context).Should().BeFalse();
    }

    [Fact]
    public void ReturnFalseWhenIsBodyIsTrue()
    {
        var context = CreateContext();
        context.IsBody = true;

        _parser.CanParse("# comment", context).Should().BeFalse();
    }

    [Fact]
    public void ExtractRequestNameFromNameDirective()
    {
        var context = CreateContext();

        _parser.Parse("# @name MyRequest", context);

        context.RequestName.Should().Be("MyRequest");
    }

    [Fact]
    public void NotSetNameForPlainComment()
    {
        var context = CreateContext();

        _parser.Parse("# just a plain comment", context);

        context.RequestName.Should().BeEmpty();
    }
}
