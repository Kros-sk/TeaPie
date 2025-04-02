using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.StructureExploration;
using TeaPie.StructureExploration.Paths;
using static Xunit.Assert;

namespace TeaPie.Tests.StructureExploration;

public class CollectionStructureExplorerShould
{
    public CollectionStructureExplorerShould()
    {
        Directory.CreateDirectory(Constants.SystemTemporaryFolderPath);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ThrowProperExceptionWhenInvalidPathIsGiven(bool emptyPath)
    {
        var structureExplorer = GetStructureExplorer();
        var builder = new ApplicationContextBuilder();

        if (emptyPath)
        {
            Throws<InvalidOperationException>(() => structureExplorer.Explore(builder.WithPath(string.Empty).Build()));
        }
        else
        {
            Throws<InvalidOperationException>(() => structureExplorer.Explore(
                builder.WithPath(
                    $"{Path.GetPathRoot(Environment.SystemDirectory)}{Path.DirectorySeparatorChar}Invalid-{Guid.NewGuid()}")
                .Build()));
        }
    }

    [Fact]
    public void ThrowProperExceptionWhenTestCasePathIsGiven()
    {
        var structureExplorer = GetStructureExplorer();
        var builder = new ApplicationContextBuilder();
        var testCasePath = Path.Combine(
            Environment.CurrentDirectory,
            StructureExplorationIndex.RootFolderName,
            StructureExplorationIndex.TestCasesRelativePaths[0]);

        Throws<InvalidOperationException>(() => structureExplorer.Explore(builder.WithPath(testCasePath).Build()));
    }

    [Fact]
    public void CreateTeaPieFolder()
    {
        var builder = new ApplicationContextBuilder();
        var collectionPath = Path.Combine(
            Environment.CurrentDirectory,
            StructureExplorationIndex.CollectionFolderRelativePath);

        var pathProvider = new PathProvider();
        pathProvider.UpdatePaths(collectionPath, Constants.SystemTemporaryFolderPath);
        var structureExplorer = GetStructureExplorer(pathProvider);

        var structure = structureExplorer.Explore(builder.WithPath(collectionPath).Build());
        var teaPieFolderPath = Constants.SystemTemporaryFolderPath;

        True(structure.TryGetFolder(teaPieFolderPath, out var folder));
        NotNull(folder);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReturnEmptyListOfTestCasesWhenExploringFoldersWithoutAnyTestCases(bool nestedFolders)
    {
        var builder = new ApplicationContextBuilder();
        string tempDirectoryPath;
        if (nestedFolders)
        {
            tempDirectoryPath = Path.Combine(
                Environment.CurrentDirectory, StructureExplorationIndex.CollectionFolderRelativePath, "ThirdFolder");
        }
        else
        {
            tempDirectoryPath = Path.Combine(
                Environment.CurrentDirectory, StructureExplorationIndex.CollectionFolderRelativePath, "EmptyFolder");
        }

        var pathProvider = new PathProvider();
        pathProvider.UpdatePaths(tempDirectoryPath, Constants.SystemTemporaryFolderPath);
        var structureExplorer = GetStructureExplorer(pathProvider);

        var testCases = structureExplorer.Explore(builder.WithPath(tempDirectoryPath).Build()).TestCases;

        Empty(testCases);
    }

    [Fact]
    public void ReturnTestCasesInCorrectOrder()
    {
        var builder = new ApplicationContextBuilder();
        var tempDirectoryPath = Path.Combine(
            Environment.CurrentDirectory, StructureExplorationIndex.CollectionFolderRelativePath);

        var pathProvider = new PathProvider();
        pathProvider.UpdatePaths(tempDirectoryPath, Constants.SystemTemporaryFolderPath);
        var structureExplorer = GetStructureExplorer(pathProvider);

        var testCasesOrder = structureExplorer.Explore(builder.WithPath(tempDirectoryPath).Build()).TestCases
            .ToList();

        Equal(StructureExplorationIndex.TestCasesRelativePaths.Length, testCasesOrder.Count);

        for (var i = 0; i < StructureExplorationIndex.TestCasesRelativePaths.Length; i++)
        {
            Equal(
                Path.Combine(
                    tempDirectoryPath,
                    StructureExplorationIndex.TestCasesRelativePaths[i]
                        .TrimRootPath(StructureExplorationIndex.CollectionFolderName)),
                testCasesOrder[i].RequestsFile.Path);
        }
    }

    [Fact]
    public void AssignPreRequestAndPostResponseScriptsOfTestCasesCorrectly()
    {
        var builder = new ApplicationContextBuilder();
        var tempDirectoryPath = Path.Combine(
            Environment.CurrentDirectory, StructureExplorationIndex.CollectionFolderRelativePath);

        var pathProvider = new PathProvider();
        pathProvider.UpdatePaths(tempDirectoryPath, Constants.SystemTemporaryFolderPath);
        var structureExplorer = GetStructureExplorer(pathProvider);

        var testCasesOrder = structureExplorer.Explore(builder.WithPath(tempDirectoryPath).Build()).TestCases
            .ToList();

        Equal(StructureExplorationIndex.TestCasesRelativePaths.Length, testCasesOrder.Count);

        bool hasPreRequest, hasPostResponse;
        string path;
        TestCase testCase;
        for (var i = 0; i < StructureExplorationIndex.TestCasesRelativePaths.Length; i++)
        {
            testCase = testCasesOrder[i];
            hasPreRequest = testCase.PreRequestScripts.Any();
            hasPostResponse = testCase.PostResponseScripts.Any();

            path = testCase.RequestsFile.RelativePath.TrimRootPath(StructureExplorationIndex.CollectionFolderRelativePath);

            Equal(StructureExplorationIndex.TestCasesScriptsMap[path].hasPreRequest, hasPreRequest);
            Equal(StructureExplorationIndex.TestCasesScriptsMap[path].hasPostResponse, hasPostResponse);
        }
    }

    private static CollectionStructureExplorer GetStructureExplorer(IPathProvider? pathProvider = null)
        => new(pathProvider ?? Substitute.For<IPathProvider>(), Substitute.For<ILogger<CollectionStructureExplorer>>());
}
