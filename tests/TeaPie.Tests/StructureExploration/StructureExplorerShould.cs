using FluentAssertions;
using TeaPie.Helpers;
using TeaPie.StructureExploration;

namespace TeaPie.Tests.StructureExploration;

public class StructureExplorerShould
{
    private const string RootFolderName = "Demo";

    private static readonly string[] _testCasesPaths = [
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

    private static readonly Dictionary<string, (bool hasPreRequest, bool hasPostResponse)> _testCasesScriptsMap = new()
    {
        {_testCasesPaths[0], (false, false)},
        {_testCasesPaths[1], (true, false)},
        {_testCasesPaths[2], (true, false)},
        {_testCasesPaths[3], (false, false)},
        {_testCasesPaths[4], (false, false)},
        {_testCasesPaths[5], (false, true)},
        {_testCasesPaths[6], (true, true)},
        {_testCasesPaths[7], (true, true)},
        {_testCasesPaths[8], (true, true)}
    };

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
            structureExplorer.Invoking(se =>
                se.ExploreFileSystem(
                    $"{Path.GetPathRoot(Environment.SystemDirectory)}{Path.DirectorySeparatorChar}Invalid-{Guid.NewGuid()}"))
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

    [Fact]
    public void FoundPreRequestAndPostResponseScriptsOfTestCasesShouldReflectReality()
    {
        var tempDirectoryPath = Path.Combine(Environment.CurrentDirectory, RootFolderName);
        var structureExplorer = new StructureExplorer();

        var testCasesOrder = structureExplorer.ExploreFileSystem(tempDirectoryPath).Values.ToList();

        testCasesOrder.Count.Should().Be(_testCasesPaths.Length);

        bool hasPreRequest, hasPostResponse;
        string path;
        TestCase testCase;
        for (var i = 0; i < _testCasesPaths.Length; i++)
        {
            testCase = testCasesOrder[i];
            hasPreRequest = testCase.PreRequestScripts.Any();
            hasPostResponse = testCase.PostResponseScripts.Any();

            path = testCase.Request.RelativePath.TrimRootPath(RootFolderName);

            hasPreRequest.Should().Be(_testCasesScriptsMap[path].hasPreRequest);
            hasPostResponse.Should().Be(_testCasesScriptsMap[path].hasPostResponse);
        }
    }
}
