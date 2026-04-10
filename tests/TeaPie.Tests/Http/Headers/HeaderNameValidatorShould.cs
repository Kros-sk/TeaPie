using System.Data;
using FluentAssertions;
using TeaPie.Http.Headers;

namespace TeaPie.Tests.Http.Headers;

public class HeaderNameValidatorShould
{
    [Theory]
    [InlineData("Content-Type")]
    [InlineData("X-Custom-Header")]
    [InlineData("Accept")]
    public void NotThrowForValidHeaderName(string headerName)
    {
        var act = () => HeaderNameValidator.CheckName(headerName);
        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowForEmptyHeaderName()
    {
        var act = () => HeaderNameValidator.CheckName(string.Empty);
        act.Should().Throw<SyntaxErrorException>();
    }

    [Fact]
    public void ThrowForHeaderNameWithSpaces()
    {
        var act = () => HeaderNameValidator.CheckName("Content Type");
        act.Should().Throw<SyntaxErrorException>();
    }

    [Fact]
    public void NotThrowForValidHeaderValue()
    {
        var act = () => HeaderNameValidator.CheckValue("application/json");
        act.Should().NotThrow();
    }

    [Fact]
    public void NotThrowForCheckHeaderWithValidNameAndValue()
    {
        var act = () => HeaderNameValidator.CheckHeader("Content-Type", "application/json");
        act.Should().NotThrow();
    }
}
