using FluentAssertions;
using TeaPie.StructureExploration;

namespace TeaPie.Tests.StructureExploration;

public class ExternalFileShould
{
    [Fact]
    public void SetPathCorrectly()
    {
        var file = new ExternalFile("/external/path/script.csx");

        file.Path.Should().Be("/external/path/script.csx");
    }

    [Fact]
    public void ExtractNameCorrectly()
    {
        var file = new ExternalFile("/external/path/script.csx");

        file.Name.Should().Be("script.csx");
    }
}
