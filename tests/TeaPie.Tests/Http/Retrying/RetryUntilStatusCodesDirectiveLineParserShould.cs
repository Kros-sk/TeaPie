using FluentAssertions;
using TeaPie.Http.Parsing;

namespace TeaPie.Tests.Http.Retrying;

public class RetryUntilStatusCodesDirectiveLineParserShould
{
    private readonly RetryUntilStatusCodesLineParser _parser = new();

    private static HttpParsingContext CreateContext()
    {
        using var msg = new HttpRequestMessage();
        return new HttpParsingContext(msg.Headers);
    }

    [Fact]
    public void ReturnTrueForMatchingLine()
    {
        var context = CreateContext();

        _parser.CanParse("## RETRY-UNTIL-STATUS: [200, 201]", context).Should().BeTrue();
    }

    [Fact]
    public void ReturnFalseForNonMatchingLine()
    {
        var context = CreateContext();

        _parser.CanParse("## SOMETHING-ELSE: value", context).Should().BeFalse();
    }

    [Fact]
    public void ExtractStatusCodesIntoContext()
    {
        var context = CreateContext();

        _parser.Parse("## RETRY-UNTIL-STATUS: [200, 201]", context);

        context.RetryUntilStatusCodes.Should().HaveCount(2);
        context.RetryUntilStatusCodes.Should().Contain(System.Net.HttpStatusCode.OK);
        context.RetryUntilStatusCodes.Should().Contain(System.Net.HttpStatusCode.Created);
    }
}
