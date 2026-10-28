using FluentAssertions;
using TeaPie.Templating;

namespace TeaPie.Tests.Templating;

public class TextEditApplierShould
{
    [Fact]
    public void ReturnTheOriginalContentUnchangedWhenThereAreNoEdits()
    {
        var result = TextEditApplier.Apply("hello world", []);

        result.Should().Be("hello world");
    }

    [Fact]
    public void ApplyASingleEditAtTheCorrectPosition()
    {
        var edits = new[] { new TextEdit(6, 5, "TEAPIE") };

        var result = TextEditApplier.Apply("hello world", edits);

        result.Should().Be("hello TEAPIE");
    }

    [Fact]
    public void ApplyMultipleNonOverlappingEditsRegardlessOfInputOrder()
    {
        var edits = new[]
        {
            new TextEdit(6, 5, "WORLD"),
            new TextEdit(0, 5, "HELLO")
        };

        var result = TextEditApplier.Apply("hello world", edits);

        result.Should().Be("HELLO WORLD");
    }

    [Fact]
    public void SupportReplacementTextOfADifferentLengthThanTheOriginalSpan()
    {
        var edits = new[] { new TextEdit(0, 5, "HI") };

        var result = TextEditApplier.Apply("hello world", edits);

        result.Should().Be("HI world");
    }
}
