using FluentAssertions;
using TeaPie.StructureExploration;
using File = System.IO.File;

namespace TeaPie.Tests.StructureExploration;

public class StructureExplorerShould : IDisposable
{
    //Testing folder structure:
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

    private string _tempDirectoryPath = string.Empty;

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
        CreateTestDirectory(false, false);
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
    public void FoldersWithoutTestCaseShouldReturnEmptyListOfTestCases(bool wholeStructure)
    {
        CreateTestDirectory(wholeStructure, false);
        var structureExplorer = new StructureExplorer();

        var testCases = structureExplorer.ExploreFileSystem(_tempDirectoryPath);

        testCases.Should().BeEmpty();
    }

    [Fact]
    public void FoundTestCasesShouldBeInCorrectOrder()
    {
        CreateTestDirectory(true, true);
        var structureExplorer = new StructureExplorer();

        var testCasesOrder = structureExplorer.ExploreFileSystem(_tempDirectoryPath).Keys.ToList();

        testCasesOrder.Count.Should().Be(_testCasesPaths.Length);

        for (var i = 0; i < _testCasesPaths.Length; i++)
        {
            testCasesOrder[i].Should().BeEquivalentTo(Path.Combine(_tempDirectoryPath, _testCasesPaths[i]));
        }
    }

    private void CreateTestDirectory(bool withFolders, bool withTestCases)
    {
        _tempDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectoryPath);

        if (withFolders)
        {
            CreateFolders();
        }

        if (withTestCases)
        {
            CreateTestCases();
        }
    }

    private void CreateTestCases()
    {
        foreach (var testCase in _testCasesPaths)
        {
            File.Create(Path.Combine(_tempDirectoryPath, testCase)).Dispose();
        }
    }
    private void CreateFolders()
    {
        foreach (var directory in _foldersPaths)
        {
            Directory.CreateDirectory(Path.Combine(_tempDirectoryPath, directory));
        }
    }

    /// <summary>
    /// This method is called after each test execution. So, this serves as tear-down method, which clears residual items.
    /// </summary>
    public void Dispose()
    {
        if (!string.IsNullOrEmpty(_tempDirectoryPath) && Directory.Exists(_tempDirectoryPath))
        {
            Directory.Delete(_tempDirectoryPath, true);
        }
    }
}
