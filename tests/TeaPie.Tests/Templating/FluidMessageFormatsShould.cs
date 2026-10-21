using System.Text.RegularExpressions;
using Fluid;
using FluentAssertions;
using TeaPie.Templating;

namespace TeaPie.Tests.Templating;

public class FluidMessageFormatsShould
{
    [Fact]
    public void StillReportRenderStepLimitExceededExceptionsUsingTheExpectedMarkerWord()
    {
        var parser = new FluidParser();
        var heavyTag = string.Concat(Enumerable.Repeat("{{ i }}", 50));
        parser.TryParse($"{{% for i in (1..50) %}}{heavyTag}{{% endfor %}}", out var template, out _);

        var options = new TemplateOptions { MaxSteps = 10 };
        var context = new TemplateContext(new Dictionary<string, object?>(), options);

        var act = () => template!.Render(context);

        act.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain(
                FluidMessageFormats.RenderStepLimitMarker,
                "TemplateExpander.IsRenderStepLimitExceeded relies on this exact word to detect the limit");
    }

    [Fact]
    public void StillReportParseErrorsInTheExpectedAtLineColumnFormat()
    {
        var parser = new FluidParser();

        parser.TryParse("{% badtag %}", out _, out var error);

        Regex.IsMatch(error!, FluidMessageFormats.ParseErrorPositionPattern).Should().BeTrue(
            $"FluidParseErrorMapper.PositionRegex relies on this exact format, but Fluid returned: '{error}'");
    }
}
