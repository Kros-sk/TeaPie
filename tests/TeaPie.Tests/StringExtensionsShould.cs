using FluentAssertions;

namespace TeaPie.Tests;

public class StringExtensionsShould
{
    [Fact]
    public void RemoveMatchingSuffix()
    {
        "HelloWorld".TrimSuffix("World").Should().Be("Hello");
    }

    [Fact]
    public void ReturnSameStringWhenSuffixDoesNotMatch()
    {
        "HelloWorld".TrimSuffix("Planet").Should().Be("HelloWorld");
    }

    [Fact]
    public void ReturnOriginalStringWhenSuffixIsNull()
    {
        "HelloWorld".TrimSuffix(null!).Should().Be("HelloWorld");
    }

    [Fact]
    public void ReturnOriginalStringWhenSuffixIsEmpty()
    {
        "HelloWorld".TrimSuffix(string.Empty).Should().Be("HelloWorld");
    }

    [Fact]
    public void ThrowArgumentNullExceptionWhenTextIsNull()
    {
        string text = null!;
        var act = () => text.TrimSuffix("suffix");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ReturnEmptyStringWhenTextEqualsSuffix()
    {
        "Hello".TrimSuffix("Hello").Should().Be(string.Empty);
    }
}
