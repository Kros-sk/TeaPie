using FluentAssertions;
using TeaPie.Variables;

namespace TeaPie.Tests.Variables;

public class VariableNameValidatorShould
{
    [Theory]
    [InlineData("myVariable")]
    [InlineData("my-variable")]
    [InlineData("var_123")]
    [InlineData("a")]
    [InlineData("my.var")]
    public void NotThrowForValidName(string name)
    {
        var act = () => VariableNameValidator.Resolve(name);
        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowForNullName()
    {
        var act = () => VariableNameValidator.Resolve(null);
        act.Should().Throw<VariableNameViolationException>();
    }

    [Fact]
    public void ThrowForEmptyString()
    {
        var act = () => VariableNameValidator.Resolve(string.Empty);
        act.Should().Throw<VariableNameViolationException>();
    }

    [Fact]
    public void ThrowForWhitespaceOnly()
    {
        var act = () => VariableNameValidator.Resolve("   ");
        act.Should().Throw<VariableNameViolationException>();
    }

    [Fact]
    public void ThrowForNameWithAngleBrackets()
    {
        var act = () => VariableNameValidator.Resolve("my<var>");
        act.Should().Throw<VariableNameViolationException>();
    }

    [Fact]
    public void ThrowForNameStartingWithDollarSign()
    {
        var act = () => VariableNameValidator.Resolve("$myVar");
        act.Should().Throw<VariableNameViolationException>();
    }

    [Fact]
    public void ThrowForNameWithSpaces()
    {
        var act = () => VariableNameValidator.Resolve("my var");
        act.Should().Throw<VariableNameViolationException>();
    }
}
