using Fluid;
using FluentAssertions;
using TeaPie.Templating;

namespace TeaPie.Tests.Templating;

public class TopLevelLoopWrapperShould
{
    private static readonly FluidParser Parser = new();

    [Fact]
    public void ReportTheOutputOfEachTopLevelLoopIncludingItsNestedLoops()
    {
        const string content =
            "<{% for a in (1..2) %}a{% endfor %}|{% for b in (1..1) %}b{% for n in (1..2) %}n{% endfor %}{% endfor %}>";

        var (output, reported) = WrapAndRender(content, ["a", "b"]);

        output.Should().Be("<aa|bnn>");
        reported.Should().Equal((0, "aa"), (1, "bnn"));
    }

    [Fact]
    public void ReportLoopsInsideConditionalBlocks()
    {
        const string content =
            "{% if true %}{% for a in (1..1) %}a{% endfor %}{% endif %}" +
            "{% unless false %}{% for b in (1..1) %}b{% endfor %}{% endunless %}" +
            "{% case 1 %}{% when 1 %}{% for c in (1..1) %}c{% endfor %}{% endcase %}" +
            "{% if false %}{% elsif true %}{% for d in (1..1) %}d{% endfor %}{% endif %}" +
            "{% if false %}{% else %}{% for e in (1..1) %}e{% endfor %}{% endif %}";

        var (output, reported) = WrapAndRender(content, ["a", "b", "c", "d", "e"]);

        output.Should().Be("abcde");
        reported.Should().Equal((0, "a"), (1, "b"), (2, "c"), (3, "d"), (4, "e"));
    }

    [Fact]
    public void NotReportALoopThatDidNotRun()
    {
        const string content = "{% if false %}{% for a in (1..2) %}a{% endfor %}{% endif %}x";

        var (output, reported) = WrapAndRender(content, ["a"]);

        output.Should().Be("x");
        reported.Should().BeEmpty();
    }

    [Fact]
    public void KeepWhitespaceControlAroundWrappedLoops()
    {
        const string content =
            "A\r\n\r\n{%- for a in (1..2) -%}\r\n[{{ a }}]\r\n{%- endfor -%}\r\n\r\n{%- for b in (1..1) -%}\r\n" +
            "[{{ b }}]\r\n{%- endfor -%}\r\n\r\nB";

        var (output, reported) = WrapAndRender(content, ["a", "b"]);

        output.Should().Be("A[1][2][1]B");
        reported.Should().Equal((0, "[1][2]"), (1, "[1]"));
    }

    [Theory]
    [InlineData("a")]
    [InlineData("a,b,c")]
    [InlineData("a,x")]
    [InlineData("b,a")]
    public void FailWhenTopLevelLoopsDoNotMatchTheExpectedOnes(string expectedLoopVariableNames)
    {
        var template = Parser.Parse("{% for a in (1..1) %}{% endfor %}{% for b in (1..1) %}{% endfor %}");

        var act = () => TopLevelLoopWrapper.Wrap(template, expectedLoopVariableNames.Split(','), (_, _) => { });

        act.Should().Throw<InvalidOperationException>().WithMessage("Templating error: internal error*");
    }

    private static (string Output, List<(int Index, string Output)> Reported) WrapAndRender(
        string content, string[] expectedLoopVariableNames)
    {
        var reported = new List<(int, string)>();
        var template = TopLevelLoopWrapper.Wrap(
            Parser.Parse(content), expectedLoopVariableNames, (index, output) => reported.Add((index, output)));

        return (template.Render(new TemplateContext()), reported);
    }
}
