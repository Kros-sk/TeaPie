using FluentAssertions;
using TeaPie.StructureExploration;

namespace TeaPie.Tests.StructureExploration;

public class FolderShould
{
    [Fact]
    public void SetPropertiesCorrectly()
    {
        var folder = new Folder("/home/user/project", "project", "project");

        folder.Path.Should().Be("/home/user/project");
        folder.RelativePath.Should().Be("project");
        folder.Name.Should().Be("project");
    }

    [Fact]
    public void SupportRecordEquality()
    {
        var folder1 = new Folder("/path", "rel", "name");
        var folder2 = new Folder("/path", "rel", "name");

        folder1.Should().Be(folder2);
    }

    [Fact]
    public void AllowNullParentFolder()
    {
        var folder = new Folder("/path", "rel", "name", null);

        folder.ParentFolder.Should().BeNull();
    }
}
