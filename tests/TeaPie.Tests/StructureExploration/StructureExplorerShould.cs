using TeaPie.StructureExploration;
using File = System.IO.File;

namespace TeaPie.Tests.StructureExploration;

public class StructureExplorerShould
{
    //Testing file structure:
    //root/
    // ├── FirstFolder /
    // │   ├── FirstFolderInFirstFolder /
    // │   │   ├── Seed.http
    // │   │   └── Test1.1.1.http
    // │   ├── SecondFolderInFirstFolder /
    // │   │   ├── FFinSFinFF /
    // │   │   │   └── Test1.2.1.1.http
    // │   │   ├── Test1.2.1.http
    // │   │   └── Test1.2.2.http
    // ├── SecondFolder /
    // │   └── FirstFolderInSecondFolder /
    // │       └── ATest.http
    // ├── ThirdFolder /
    // ├── AZeroLevelTest.http
    // └── ZeroLevelTest.http

    private readonly string[] _foldersPaths = [
        "FirstFolder",
        "SecondFolder",
        "ThirdFolder",
        Path.Combine("FirstFolder", "FirstFolderInFirstFolder"),
        Path.Combine("FirstFolder", "SecondFolderInFirstFolder"),
        Path.Combine("FirstFolder", "SecondFolderInFirstFolder", "FFinSFinFF"),
        Path.Combine("SecondFolder", "FirstFolderInSecondFolder")
    ];

    private readonly string[] _testCasesPaths = [
        Path.Combine("FirstFolder", "FirstFolderInFirstFolder", $"Seed{Constants.RequestFileExtension}"),
        Path.Combine("FirstFolder", "FirstFolderInFirstFolder", $"Test1.1.1{Constants.RequestFileExtension}"),
        Path.Combine("FirstFolder", "SecondFolderInFirstFolder", "FFinSFinFF",
            $"Test1.2.1.1{Constants.RequestFileExtension}"),
        Path.Combine("FirstFolder", "SecondFolderInFirstFolder", $"Test1.2.1{Constants.RequestFileExtension}"),
        Path.Combine("FirstFolder", "SecondFolderInFirstFolder", $"Test1.2.2{Constants.RequestFileExtension}"),
        Path.Combine("SecondFolder", "FirstFolderInSecondFolder", $"ATest{Constants.RequestFileExtension}"),
        Path.Combine($"AZeroLevelTest{Constants.RequestFileExtension}"),
        Path.Combine($"ZeroLevelTest{Constants.RequestFileExtension}")
    ];

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void InvalidPathShouldThrowException(bool emptyPath)
    {
        var tempDirectory = CreateTestDirectory(false, false);
        var structureExplorer = new StructureExplorer();

        var testCases = structureExplorer.ExploreFileSystem(tempDirectory);

        Assert.Empty(testCases);

        if (emptyPath)
        {
            Assert.Throws<ArgumentException>(() => structureExplorer.ExploreFileSystem(string.Empty));
        }
        else
        {
            Assert.Throws<DirectoryNotFoundException>(
                () => structureExplorer.ExploreFileSystem($"C:\\{Guid.NewGuid()}-Invalid-{Guid.NewGuid()}"));
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FoldersWithoutTestCaseShouldReturnEmptyListOfTestCases(bool wholeStructure)
    {
        var tempDirectory = CreateTestDirectory(wholeStructure, false);
        var structureExplorer = new StructureExplorer();

        var testCases = structureExplorer.ExploreFileSystem(tempDirectory);

        Assert.Empty(testCases);

        Directory.Delete(tempDirectory, true);
    }

    [Fact]
    public void FoundTestCasesShouldBeInCorrectOrder()
    {
        var tempDirectory = CreateTestDirectory(true, true);
        var structureExplorer = new StructureExplorer();

        var testCasesOrder = structureExplorer.ExploreFileSystem(tempDirectory).Keys.ToList();

        Assert.Equal(_testCasesPaths.Length, testCasesOrder.Count);

        for (var i = 0; i < _testCasesPaths.Length; i++)
        {
            Assert.Equal(Path.Combine(tempDirectory, _testCasesPaths[i]), testCasesOrder[i]);
        }

        Directory.Delete(tempDirectory, true);
    }

    private string CreateTestDirectory(bool withFolders, bool withTestCases)
    {
        var rootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(rootPath);

        if (withFolders)
        {
            CreateFolders(rootPath);
        }

        if (withTestCases)
        {
            CreateTestCases(rootPath);
        }

        return rootPath;
    }

    private void CreateTestCases(string rootPath)
    {
        foreach (var testCase in _testCasesPaths)
        {
            File.Create(Path.Combine(rootPath, testCase)).Dispose();
        }
    }
    private void CreateFolders(string rootPath)
    {
        foreach (var directory in _foldersPaths)
        {
            Directory.CreateDirectory(Path.Combine(rootPath, directory));
        }
    }
}
