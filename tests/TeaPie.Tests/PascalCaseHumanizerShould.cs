using FluentAssertions;

namespace TeaPie.Tests;

public class PascalCaseHumanizerShould
{
    [Theory]
    [InlineData("HelloWorld", "Hello World")]
    [InlineData("Hello", "Hello")]
    [InlineData("ThisIsATest", "This Is A Test")]
    [InlineData("hello", "hello")]
    [InlineData("ABC", "A B C")]
    public void SplitPascalCaseCorrectly(string input, string expected)
    {
        input.SplitPascalCase().Should().Be(expected);
    }

    [Fact]
    public void ReturnEmptyStringForEmptyInput()
    {
        string.Empty.SplitPascalCase().Should().Be(string.Empty);
    }
}
