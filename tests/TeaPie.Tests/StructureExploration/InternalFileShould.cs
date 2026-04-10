using FluentAssertions;
using TeaPie.StructureExploration;

namespace TeaPie.Tests.StructureExploration;

public class InternalFileShould
{
    [Fact]
    public void SetPropertiesCorrectlyViaCreate()
    {
        var folder = new Folder("/home/user/project", "project", "project");

        var file = InternalFile.Create("/home/user/project/test-req.http", folder);

        file.Path.Should().Be("/home/user/project/test-req.http");
        file.Name.Should().Be("test-req.http");
        file.ParentFolder.Should().Be(folder);
    }

    [Fact]
    public void ComputeRelativePathFromFolderAndFileName()
    {
        var folder = new Folder("/home/user/project/sub", "project/sub", "sub");

        var file = InternalFile.Create("/home/user/project/sub/request.http", folder);

        file.RelativePath.Should().Be(System.IO.Path.Combine("project/sub", "request.http"));
    }

    [Fact]
    public void SetParentFolderCorrectly()
    {
        var parent = new Folder("/root", "root", "root");
        var folder = new Folder("/root/child", "root/child", "child", parent);

        var file = InternalFile.Create("/root/child/file.http", folder);

        file.ParentFolder.Should().Be(folder);
        file.ParentFolder.ParentFolder.Should().Be(parent);
    }
}
