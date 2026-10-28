using FluentAssertions;
using TeaPie.Templating;

namespace TeaPie.Tests.Templating;

public class LoopBlockScannerShould
{
    [Fact]
    public void FindALoopBlockWithItsVariableSourceAndBody()
    {
        const string content = "before\n{% for car in Cars %}BODY{% endfor %}\nafter";

        var blocks = new LoopBlockScanner().FindLoopBlocks(content);

        var block = blocks.Should().ContainSingle().Which;
        block.LoopVariableName.Should().Be("car");
        block.SourceExpression.Should().Be("Cars");
        block.Body.Should().Be("BODY");
        block.SourceExpressionStartIndex.Should().Be(content.IndexOf("Cars", StringComparison.Ordinal));
        block.SourceExpressionRawLength.Should().Be("Cars".Length);
    }

    [Theory]
    [InlineData("{%- for car in Cars -%}BODY{%- endfor -%}", "car:Cars")]
    [InlineData("{% for x in (\"a%}b\", \"c\") %}BODY{% endfor %}", "x:(\"a%}b\", \"c\")")]
    [InlineData("{% for a in As %}A{% endfor %}mid{% for b in Bs %}B{% endfor %}", "a:As", "b:Bs")]
    [InlineData("{% for a in As %}{% for b in a.Bs %}{% for c in b.Cs %}BODY{% endfor %}{% endfor %}{% endfor %}",
        "a:As", "b:a.Bs", "c:b.Cs")]
    [InlineData("{% raw %}{% for x in y %}{% endraw %}")]
    [InlineData("{%- raw -%}{% endfor %}{% for x in y %}{%- endraw -%}")]
    [InlineData("{% comment %}{% for x in y %}\n{% endfor %}{% endfor %}{% endcomment %}")]
    [InlineData("{% for a in As %}{% raw %}{% endfor %}{% endraw %}{% endfor %}{% comment %}{% for{% endcomment %}",
        "a:As")]
    public void FindEveryLoopBlockIncludingNestedOnes(string content, params string[] expected)
    {
        var blocks = new LoopBlockScanner().FindLoopBlocks(content);

        blocks.Select(block => $"{block.LoopVariableName}:{block.SourceExpression}").Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData("{% for car in Cars %}BODY, no closing tag", "*endfor*")]
    [InlineData("{% for a in As %}{% for b in Bs %}BODY{% endfor %}", "*endfor*for a in As*")]
    [InlineData("before\n{% endfor %}\nafter", "*endfor*")]
    [InlineData("{% for car Cars %}BODY{% endfor %}", "*")]
    public void ThrowForUnbalancedOrMalformedLoopTags(string content, string expectedMessage)
    {
        var act = () => new LoopBlockScanner().FindLoopBlocks(content);

        act.Should().Throw<InvalidOperationException>().WithMessage(expectedMessage);
    }
}
