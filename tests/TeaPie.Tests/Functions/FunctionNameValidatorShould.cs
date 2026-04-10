using FluentAssertions;
using TeaPie.Functions;

namespace TeaPie.Tests.Functions;

public class FunctionNameValidatorShould
{
    [Theory]
    [InlineData("$myFunction")]
    [InlineData("$func-123")]
    [InlineData("$a")]
    public void NotThrowForValidName(string name)
    {
        var act = () => FunctionNameValidator.Resolve(name);
        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowForNullName()
    {
        var act = () => FunctionNameValidator.Resolve(null);
        act.Should().Throw<FunctionNameViolationException>();
    }

    [Fact]
    public void ThrowForEmptyString()
    {
        var act = () => FunctionNameValidator.Resolve(string.Empty);
        act.Should().Throw<FunctionNameViolationException>();
    }

    [Fact]
    public void ThrowForWhitespaceOnly()
    {
        var act = () => FunctionNameValidator.Resolve("   ");
        act.Should().Throw<FunctionNameViolationException>();
    }

    [Fact]
    public void ThrowForNameWithoutDollarPrefix()
    {
        var act = () => FunctionNameValidator.Resolve("myFunc");
        act.Should().Throw<FunctionNameViolationException>();
    }

    [Fact]
    public void ThrowForNameWithSpaces()
    {
        var act = () => FunctionNameValidator.Resolve("$my func");
        act.Should().Throw<FunctionNameViolationException>();
    }

    [Fact]
    public void ThrowForNameWithAngleBrackets()
    {
        var act = () => FunctionNameValidator.Resolve("$my<func>");
        act.Should().Throw<FunctionNameViolationException>();
    }
}
