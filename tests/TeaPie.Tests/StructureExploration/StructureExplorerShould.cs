using FluentAssertions;
using TeaPie.StructureExploration;

namespace TeaPie.Tests.StructureExploration;

public class StructureExplorerShould
{
    private const string RootFolderName = "Demo";

    private readonly string[] _testCasesPaths = [
        Path.Combine("FirstFolder", "FirstFolderInFirstFolder", $"Seed{Constants.RequestFileExtension}"),
        Path.Combine("FirstFolder", "FirstFolderInFirstFolder",
            $"Test1.1.1{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine("FirstFolder", "SecondFolderInFirstFolder", "FFinSFinFF",
            $"Test1.2.1.1{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine("FirstFolder", "SecondFolderInFirstFolder",
            $"Test1.2.1{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine("FirstFolder", "SecondFolderInFirstFolder",
            $"Test1.2.2{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine("SecondFolder", "FirstFolderInSecondFolder",
            $"ATest{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine($"AZeroLevelTest{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine($"TheZeroLevelTest{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine($"ZeroLevelTest{Constants.RequestSuffix}{Constants.RequestFileExtension}")
    ];

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void InvalidPathShouldThrowException(bool emptyPath)
    {
        var structureExplorer = new StructureExplorer();

        if (emptyPath)
        {
            structureExplorer.Invoking(se => se.ExploreFileSystem(string.Empty))
                .Should().Throw<ArgumentException>();
        }
        else
        {
            structureExplorer.Invoking(se => se.ExploreFileSystem($"C:\\{Guid.NewGuid()}-Invalid-{Guid.NewGuid()}"))
                .Should().Throw<DirectoryNotFoundException>();
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FoldersWithoutTestCaseShouldReturnEmptyListOfTestCases(bool nestedFolders)
    {
        string tempDirectoryPath;
        if (nestedFolders)
        {
            tempDirectoryPath = Path.Combine(Environment.CurrentDirectory, RootFolderName, "ThirdFolder");
        }
        else
        {
            tempDirectoryPath = Path.Combine(Environment.CurrentDirectory, RootFolderName, "EmptyFolder");
        }

        var structureExplorer = new StructureExplorer();

        var testCases = structureExplorer.ExploreFileSystem(tempDirectoryPath);

        testCases.Should().BeEmpty();
    }

    [Fact]
    public void FoundTestCasesShouldBeInCorrectOrder()
    {
        var tempDirectoryPath = Path.Combine(Environment.CurrentDirectory, RootFolderName);
        var structureExplorer = new StructureExplorer();

        var testCasesOrder = structureExplorer.ExploreFileSystem(tempDirectoryPath).Keys.ToList();

        testCasesOrder.Count.Should().Be(_testCasesPaths.Length);

        for (var i = 0; i < _testCasesPaths.Length; i++)
        {
            testCasesOrder[i].Should().BeEquivalentTo(Path.Combine(tempDirectoryPath, _testCasesPaths[i]));
        }
    }
}
