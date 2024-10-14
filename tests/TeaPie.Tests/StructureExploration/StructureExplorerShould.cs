using TeaPie.StructureExploration;
using File = System.IO.File;
namespace TeaPie.Tests.StructureExploration;
public class StructureExplorerShould
{
    private readonly string[] _directoriesPaths = [
        "FirstFolder",
        "SecondFolder",
        "ThirdFolder",
        Path.Combine("FirstFolder", "FirstFolderInFirtFolder"),
        Path.Combine("FirstFolder", "SecondFolderInFirtFolder"),
        Path.Combine("FirstFolder", "SecondFolderInFirtFolder", "FFinSFinFF"),
        Path.Combine("SecondFolder", "FirstFolderInSecondFolder")
    ];

    private readonly string[] _testCasesPaths = [
        Path.Combine("FirstFolder", "FirstFolderInFirtFolder", $"Seed{Constants.RequestFileExtension}"),
        Path.Combine("FirstFolder", "FirstFolderInFirtFolder", $"Test1.1.1{Constants.RequestFileExtension}"),
        Path.Combine("FirstFolder", "SecondFolderInFirtFolder", "FFinSFinFF",
            $"Test1.2.1.1{Constants.RequestFileExtension}"),
        Path.Combine("FirstFolder", "SecondFolderInFirtFolder", $"Test1.2.1{Constants.RequestFileExtension}"),
        Path.Combine("FirstFolder", "SecondFolderInFirtFolder", $"Test1.2.2{Constants.RequestFileExtension}"),
        Path.Combine("SecondFolder", "FirstFolderInSecondFolder", $"ATest{Constants.RequestFileExtension}"),
        Path.Combine($"AZeroLevelTest{Constants.RequestFileExtension}"),
        Path.Combine($"ZeroLevelTest{Constants.RequestFileExtension}")
    ];

    [Fact]
    public void TestCasesShouldBeInCorrectOrder()
    {
        var tempDirectory = CreateTestDirectory();
        IStructureExplorer structureExplorer = new StructureExplorer();

        var testCasesOrder = structureExplorer.ExploreFileSystem(tempDirectory).Keys.ToList();

        Assert.Equal(_testCasesPaths.Length, testCasesOrder.Count);

        for (var i = 0; i < _testCasesPaths.Length; i++)
        {
            Assert.Equal(Path.Combine(tempDirectory, _testCasesPaths[i]), testCasesOrder[i]);
        }

        Directory.Delete(tempDirectory, true);
    }

    private string CreateTestDirectory()
    {
        var rootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        foreach (var directory in _directoriesPaths)
        {
            Directory.CreateDirectory(Path.Combine(rootPath, directory));
        }

        foreach (var testCase in _testCasesPaths)
        {
            File.Create(Path.Combine(rootPath, testCase)).Dispose();
        }

        //Created file structure:
        //root/
        // ├── FirstFolder /
        // │   ├── FirstFolderInFirtFolder /
        // │   │   ├── Seed.http
        // │   │   └── Test1.1.1.http
        // │   ├── SecondFolderInFirtFolder /
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

        return rootPath;
    }
}
