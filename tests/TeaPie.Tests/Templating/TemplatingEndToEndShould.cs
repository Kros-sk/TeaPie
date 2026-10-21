using System.Text.RegularExpressions;
using FluentAssertions;
using static TeaPie.Tests.Templating.TemplatingTestHelpers;

namespace TeaPie.Tests.Templating;

public partial class TemplatingEndToEndShould
{
    [Fact]
    public void RoundTripEveryDemoRequestFileWithoutLoopTagsByteIdentically()
    {
        var demoRoot = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "demo");
        var files = Directory.GetFiles(demoRoot, "*.http", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(demoRoot, "*.tp", SearchOption.AllDirectories));
        var expander = CreateExpander();
        var filesWithoutFluidTags = 0;

        foreach (var file in files)
        {
            var original = File.ReadAllText(file);

            if (original.Contains("{%", StringComparison.Ordinal))
            {
                SupportedFluidTagRegex().IsMatch(original).Should().BeTrue(
                    $"file '{file}' contains '{{%' but is only exempted from the byte-identical " +
                    "round-trip check when it uses a real, supported Fluid tag (for/if/unless/assign/" +
                    "raw) - an unsupported or malformed '{{%' must not be silently skipped");
                continue;
            }

            filesWithoutFluidTags++;
            var expanded = expander.Expand(original, file);
            expanded.Should().Be(original, $"file '{file}' does not contain '{{%' and must be returned unchanged");
        }

        filesWithoutFluidTags.Should().BeGreaterThan(0);
    }

    [GeneratedRegex(@"\{%-?\s*(for|endfor|if|elsif|else|endif|unless|endunless|assign|raw|endraw)\b")]
    private static partial Regex SupportedFluidTagRegex();
}
