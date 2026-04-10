using FluentAssertions;
using StructureFile = TeaPie.StructureExploration.File;

namespace TeaPie.Tests.StructureExploration;

public class FileShould
{
    [Theory]
    [InlineData("/home/user/test.http", "test.http")]
    [InlineData("folder/subfolder/file.csx", "file.csx")]
    public void ExtractFileNameFromPath(string path, string expectedName)
    {
        var file = new StructureFile(path);

        file.Name.Should().Be(expectedName);
    }

    [Fact]
    public void ReturnPathWhenRelativePathIsEmpty()
    {
        var file = new StructureFile("/home/user/test.http");

        file.GetDisplayPath().Should().Be("/home/user/test.http");
    }

    [Fact]
    public void ReturnRelativePathWhenNotEmpty()
    {
        var file = new StructureFile("/home/user/test.http", "user/test.http");

        file.GetDisplayPath().Should().Be("user/test.http");
    }

    [Fact]
    public void ReturnTrueWhenFileIsUnderRoot()
    {
        StructureFile.BelongsTo("/home/user/project/test.http", "/home/user/project")
            .Should().BeTrue();
    }

    [Fact]
    public void ReturnFalseWhenFileIsNotUnderRoot()
    {
        StructureFile.BelongsTo("/other/path/test.http", "/home/user/project")
            .Should().BeFalse();
    }

    [Fact]
    public void TrimWhitespaceInBelongsTo()
    {
        StructureFile.BelongsTo("  /home/user/project/test.http", "  /home/user/project")
            .Should().BeTrue();
    }
}
