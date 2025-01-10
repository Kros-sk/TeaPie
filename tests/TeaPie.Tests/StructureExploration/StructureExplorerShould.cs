using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.StructureExploration;

namespace TeaPie.Tests.StructureExploration;

public class StructureExplorerShould
{
    private const string RootFolderName = "Structure";

    private readonly string _rootFolderRelativePath = Path.Combine("Demo", "Structure");

    private static readonly string[] _testCasesPaths = [
        Path.Combine(RootFolderName, "FirstFolder", "FirstFolderInFirstFolder", $"Seed{Constants.RequestFileExtension}"),
        Path.Combine(RootFolderName, "FirstFolder", "FirstFolderInFirstFolder",
            $"Test1.1.1{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine(RootFolderName, "FirstFolder", "SecondFolderInFirstFolder", "FFinSFinFF",
            $"Test1.2.1.1{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine(RootFolderName, "FirstFolder", "SecondFolderInFirstFolder",
            $"Test1.2.1{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine(RootFolderName, "FirstFolder", "SecondFolderInFirstFolder",
            $"Test1.2.2{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine(RootFolderName, "SecondFolder", "FirstFolderInSecondFolder",
            $"ATest{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine(RootFolderName, $"AZeroLevelTest{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine(RootFolderName, $"TheZeroLevelTest{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine(RootFolderName, $"ZeroLevelTest{Constants.RequestSuffix}{Constants.RequestFileExtension}")
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
    public void ThrowProperExceptionWhenInvalidPathIsGiven(bool emptyPath)
    {
        var structureExplorer = GetStructureExplorer();

        if (emptyPath)
        {
            structureExplorer.Invoking(se => se.ExploreCollectionStructure(string.Empty))
                .Should().Throw<InvalidOperationException>();
        }
        else
        {
            structureExplorer.Invoking(se =>
                se.ExploreCollectionStructure(
                    $"{Path.GetPathRoot(Environment.SystemDirectory)}{Path.DirectorySeparatorChar}Invalid-{Guid.NewGuid()}"))
                .Should().Throw<DirectoryNotFoundException>();
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReturnEmptyListOfTestCasesWhenExploringFoldersWithoutAnyTestCases(bool nestedFolders)
    {
        string tempDirectoryPath;
        if (nestedFolders)
        {
            tempDirectoryPath = Path.Combine(Environment.CurrentDirectory, _rootFolderRelativePath, "ThirdFolder");
        }
        else
        {
            tempDirectoryPath = Path.Combine(Environment.CurrentDirectory, _rootFolderRelativePath, "EmptyFolder");
        }

        var structureExplorer = GetStructureExplorer();

        var testCases = structureExplorer.ExploreCollectionStructure(tempDirectoryPath).TestCases;

        testCases.Should().BeEmpty();
    }

    [Fact]
    public void ReturnTestCasesInCorrectOrder()
    {
        var tempDirectoryPath = Path.Combine(Environment.CurrentDirectory, _rootFolderRelativePath);
        var structureExplorer = GetStructureExplorer();

        var testCasesOrder = structureExplorer.ExploreCollectionStructure(tempDirectoryPath).TestCases.ToList();

        testCasesOrder.Count.Should().Be(_testCasesPaths.Length);

        for (var i = 0; i < _testCasesPaths.Length; i++)
        {
            testCasesOrder[i].RequestsFile.Path.Should().BeEquivalentTo(
                Path.Combine(tempDirectoryPath, _testCasesPaths[i].TrimRootPath(RootFolderName)));
        }
    }

    [Fact]
    public void AssignPreRequestAndPostResponseScriptsOfTestCasesCorrectly()
    {
        var tempDirectoryPath = Path.Combine(Environment.CurrentDirectory, _rootFolderRelativePath);
        var structureExplorer = GetStructureExplorer();

        var testCasesOrder = structureExplorer.ExploreCollectionStructure(tempDirectoryPath).TestCases.ToList();

        testCasesOrder.Count.Should().Be(_testCasesPaths.Length);

        bool hasPreRequest, hasPostResponse;
        string path;
        TestCase testCase;
        for (var i = 0; i < _testCasesPaths.Length; i++)
        {
            testCase = testCasesOrder[i];
            hasPreRequest = testCase.PreRequestScripts.Any();
            hasPostResponse = testCase.PostResponseScripts.Any();

            path = testCase.RequestsFile.RelativePath.TrimRootPath(_rootFolderRelativePath);

            hasPreRequest.Should().Be(_testCasesScriptsMap[path].hasPreRequest);
            hasPostResponse.Should().Be(_testCasesScriptsMap[path].hasPostResponse);
        }
    }

    private static StructureExplorer GetStructureExplorer()
     => new(Substitute.For<ILogger<StructureExplorer>>());
}
