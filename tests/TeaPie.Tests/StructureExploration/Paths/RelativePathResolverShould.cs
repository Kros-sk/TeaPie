using FluentAssertions;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.Tests.StructureExploration.Paths;

public class RelativePathResolverShould
{
    private readonly RelativePathResolver _resolver = new();

    [Theory]
    [InlineData("sub/file.txt")]
    [InlineData("file.txt")]
    [InlineData("a/b/c.json")]
    public void ReturnTrueForRelativePath(string path)
    {
        _resolver.CanResolve(path).Should().BeTrue();
    }

    [Theory]
    [InlineData("/home/user/file.txt")]
    [InlineData("/absolute/path")]
    public void ReturnFalseForAbsolutePath(string path)
    {
        _resolver.CanResolve(path).Should().BeFalse();
    }

    [Fact]
    public void CombineRelativePathWithBasePath()
    {
        var basePath = Path.Combine(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar), "base");
        var relativePath = Path.Combine("sub", "file.txt");

        var result = _resolver.ResolvePath(relativePath, basePath);

        result.Should().Be(Path.GetFullPath(Path.Combine(basePath, relativePath)));
    }
}
