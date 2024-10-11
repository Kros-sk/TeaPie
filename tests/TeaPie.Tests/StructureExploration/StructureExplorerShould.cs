using TeaPie.StructureExploration;
using File = System.IO.File;
namespace TeaPie.Tests.StructureExploration;
public class StructureExplorerShould
{
    [Fact]
    public void TestCasesShouldBeInCorrectOrder()
    {
        var tempDirectory = CreateTestDirectory();
        IStructureExplorer structureExplorer = new StructureExplorer();

        var testCasesOrder = structureExplorer.ExploreFileSystem(tempDirectory).Keys.ToList();

        List<string> expectedOrder =
        [
            Path.Combine(tempDirectory, "FirstFolder", "FirstFolderInFirtFolder", $"Seed{Constants.RequestFileExtension}"),
            Path.Combine(tempDirectory, "FirstFolder", "FirstFolderInFirtFolder", $"Test1.1.1{Constants.RequestFileExtension}"),
            Path.Combine(tempDirectory, "FirstFolder", "SecondFolderInFirtFolder", "FFinSFinFF",
                $"Test1.2.1.1{Constants.RequestFileExtension}"),
            Path.Combine(tempDirectory, "FirstFolder", "SecondFolderInFirtFolder", $"Test1.2.1{Constants.RequestFileExtension}"),
            Path.Combine(tempDirectory, "FirstFolder", "SecondFolderInFirtFolder", $"Test1.2.2{Constants.RequestFileExtension}"),
            Path.Combine(tempDirectory, "SecondFolder", "FirstFolderInSecondFolder", $"ATest{Constants.RequestFileExtension}"),
            Path.Combine(tempDirectory, $"AZeroLevelTest{Constants.RequestFileExtension}"),
            Path.Combine(tempDirectory, $"ZeroLevelTest{Constants.RequestFileExtension}")
        ];

        Assert.Equal(expectedOrder.Count, testCasesOrder.Count);

        for (var i = 0; i < expectedOrder.Count; i++)
        {
            Assert.Equal(expectedOrder[i], testCasesOrder[i]);
        }

        Directory.Delete(tempDirectory, true);
    }

    private string CreateTestDirectory()
    {
        string rootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        //Testing structure:
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

        Directory.CreateDirectory(Path.Combine(rootPath, "FirstFolder"));
        Directory.CreateDirectory(Path.Combine(rootPath, "SecondFolder"));
        Directory.CreateDirectory(Path.Combine(rootPath, "ThirdFolder"));

        Directory.CreateDirectory(Path.Combine(rootPath, "FirstFolder", "FirstFolderInFirtFolder"));
        Directory.CreateDirectory(Path.Combine(rootPath, "FirstFolder", "SecondFolderInFirtFolder"));

        Directory.CreateDirectory(Path.Combine(rootPath, "FirstFolder", "SecondFolderInFirtFolder", "FFinSFinFF"));

        Directory.CreateDirectory(Path.Combine(rootPath, "SecondFolder", "FirstFolderInSecondFolder"));

        File.Create(
            Path.Combine(rootPath, "FirstFolder", "FirstFolderInFirtFolder", $"Seed{Constants.RequestFileExtension}"))
            .Dispose();
        File.Create(
            Path.Combine(rootPath, "FirstFolder", "FirstFolderInFirtFolder", $"Test1.1.1{Constants.RequestFileExtension}"))
            .Dispose();

        File.Create(
            Path.Combine(rootPath, "FirstFolder", "SecondFolderInFirtFolder", "FFinSFinFF",
            $"Test1.2.1.1{Constants.RequestFileExtension}"))
            .Dispose();

        File.Create(
            Path.Combine(rootPath, "FirstFolder", "SecondFolderInFirtFolder", $"Test1.2.1{Constants.RequestFileExtension}"))
            .Dispose();
        File.Create(
            Path.Combine(rootPath, "FirstFolder", "SecondFolderInFirtFolder", $"Test1.2.2{Constants.RequestFileExtension}"))
            .Dispose();

        File.Create(
            Path.Combine(rootPath, "SecondFolder", "FirstFolderInSecondFolder", $"ATest{Constants.RequestFileExtension}"))
            .Dispose();

        File.Create(Path.Combine(rootPath, $"AZeroLevelTest{Constants.RequestFileExtension}")).Dispose();
        File.Create(Path.Combine(rootPath, $"ZeroLevelTest{Constants.RequestFileExtension}")).Dispose();

        return rootPath;
    }
}
