using FluentAssertions;
using TeaPie.Scripts;

namespace TeaPie.Tests.Scripts;

public class ScriptReferenceShould
{
    [Fact]
    public void SetPropertiesCorrectly()
    {
        var reference = new ScriptReference("/real/path.csx", "/temp/path.csx");

        reference.RealPath.Should().Be("/real/path.csx");
        reference.TempPath.Should().Be("/temp/path.csx");
    }

    [Fact]
    public void DefaultIsExternalToFalse()
    {
        var reference = new ScriptReference("/real/path.csx", "/temp/path.csx");

        reference.IsExternal.Should().BeFalse();
    }

    [Fact]
    public void SupportRecordEquality()
    {
        var ref1 = new ScriptReference("/real/path.csx", "/temp/path.csx", true);
        var ref2 = new ScriptReference("/real/path.csx", "/temp/path.csx", true);

        ref1.Should().Be(ref2);
    }
}
