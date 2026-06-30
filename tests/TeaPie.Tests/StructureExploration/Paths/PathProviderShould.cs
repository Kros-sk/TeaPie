using FluentAssertions;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.Tests.StructureExploration.Paths;

public class PathProviderShould
{
    [Fact]
    public void HaveEmptyInitialPaths()
    {
        var provider = new PathProvider();

        provider.RootPath.Should().BeEmpty();
        provider.TempRootPath.Should().BeEmpty();
        provider.TeaPieFolderPath.Should().BeEmpty();
    }

    [Fact]
    public void SetRootPathAndTempRootPathOnUpdatePaths()
    {
        var provider = new PathProvider();
        var rootPath = Path.Combine("some", "root", "path");
        var tempRootPath = Path.Combine("some", "temp", "root");

        provider.UpdatePaths(rootPath, tempRootPath);

        provider.RootPath.Should().Be(rootPath);
        provider.TempRootPath.Should().Be(tempRootPath);
    }

    [Fact]
    public void UseTempRootPathAsTeaPieFolderPathWhenTeaPieFolderPathIsEmpty()
    {
        var provider = new PathProvider();
        var rootPath = Path.Combine("some", "root", "path");
        var tempRootPath = Path.Combine("some", "temp", "root");

        provider.UpdatePaths(rootPath, tempRootPath);

        provider.TeaPieFolderPath.Should().Be(tempRootPath);
    }

    [Fact]
    public void UseProvidedTeaPieFolderPathWhenNotEmpty()
    {
        var provider = new PathProvider();
        var rootPath = Path.Combine("some", "root", "path");
        var tempRootPath = Path.Combine("some", "temp", "root");
        var teaPieFolderPath = Path.Combine("custom", "teapie", "folder");

        provider.UpdatePaths(rootPath, tempRootPath, teaPieFolderPath);

        provider.TeaPieFolderPath.Should().Be(teaPieFolderPath);
    }

    [Theory]
    [InlineData("my-collection-req", "my-collection")]
    [InlineData("my-collection", "my-collection")]
    [InlineData("simple", "simple")]
    public void DeriveStructureNameFromRootPath(string fileName, string expectedStructureName)
    {
        var provider = new PathProvider();
        var rootPath = Path.Combine("some", "root", fileName + ".http");

        provider.UpdatePaths(rootPath, "temp");

        provider.StructureName.Should().Be(expectedStructureName);
    }

    [Fact]
    public void ComputeCacheFolderPathFromTeaPieFolderPath()
    {
        var provider = new PathProvider();
        var teaPieFolderPath = Path.Combine("some", "teapie");

        provider.UpdatePaths("root", "temp", teaPieFolderPath);

        provider.CacheFolderPath.Should().Be(Path.Combine(teaPieFolderPath, "cache"));
    }

    [Fact]
    public void ComputeReportsFolderPathFromTeaPieFolderPath()
    {
        var provider = new PathProvider();
        var teaPieFolderPath = Path.Combine("some", "teapie");

        provider.UpdatePaths("root", "temp", teaPieFolderPath);

        provider.ReportsFolderPath.Should().Be(Path.Combine(teaPieFolderPath, "reports"));
    }

    [Fact]
    public void ComputeVariablesFilePathEndingWithVariablesJson()
    {
        var provider = new PathProvider();
        var teaPieFolderPath = Path.Combine("some", "teapie");

        provider.UpdatePaths("root", "temp", teaPieFolderPath);

        provider.VariablesFilePath.Should().EndWith("variables.json");
    }
}
