using FluentAssertions;
using TeaPie.StructureExploration;
using StructureFile = TeaPie.StructureExploration.File;

namespace TeaPie.Tests.StructureExploration;

public class ScriptShould
{
    [Fact]
    public void SetFilePropertyCorrectly()
    {
        var file = new StructureFile("/home/user/script.csx", "script.csx");
        var script = new Script(file);

        script.File.Should().Be(file);
        script.File.Name.Should().Be("script.csx");
    }
}
