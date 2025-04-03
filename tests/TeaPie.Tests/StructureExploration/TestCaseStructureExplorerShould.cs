using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.StructureExploration;
using TeaPie.StructureExploration.Paths;
using static Xunit.Assert;

namespace TeaPie.Tests.StructureExploration;

public class TestCaseStructureExplorerShould
{
    public TestCaseStructureExplorerShould()
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
    public void ThrowProperExceptionWhenCollectionPathIsGiven()
    {
        var pathProvider = new PathProvider();
        var structureExplorer = GetStructureExplorer(pathProvider);
        var builder = new ApplicationContextBuilder();
        var collectionPath = Path.Combine(Environment.CurrentDirectory, StructureExplorationIndex.CollectionFolderRelativePath);
        pathProvider.UpdatePaths(collectionPath, string.Empty, string.Empty);

        Throws<InvalidOperationException>(() => structureExplorer.Explore(builder.WithPath(collectionPath).Build()));
    }

    [Fact]
    public void FindOnlyOneTestCase()
    {
        var builder = new ApplicationContextBuilder();
        var testCasePath = Path.Combine(
            Environment.CurrentDirectory,
            StructureExplorationIndex.RootFolderName,
            StructureExplorationIndex.TestCasesRelativePaths[0]);

        var pathProvider = new PathProvider();
        pathProvider.UpdatePaths(
            testCasePath,
            Constants.SystemTemporaryFolderPath,
            Constants.SystemTemporaryFolderPath);

        var structureExplorer = GetStructureExplorer(pathProvider);

        Single(structureExplorer.Explore(builder.WithPath(testCasePath).Build()).TestCases);
    }

    [Fact]
    public void FindTeaPieFolder()
    {
        var builder = new ApplicationContextBuilder();
        var testCasePath = Path.Combine(
            Environment.CurrentDirectory,
            StructureExplorationIndex.RootFolderName,
            StructureExplorationIndex.TestCasesRelativePaths[6]);

        var pathProvider = new PathProvider();
        pathProvider.UpdatePaths(testCasePath, Constants.SystemTemporaryFolderPath);
        var structureExplorer = GetStructureExplorer(pathProvider);

        var structure = structureExplorer.Explore(builder.WithPath(testCasePath).Build());
        var teaPieFolderPath = Constants.SystemTemporaryFolderPath;

        True(structure.TryGetFolder(teaPieFolderPath, out var folder));
        NotNull(folder);
    }

    [Fact]
    public void AssignPreRequestAndPostResponseScriptsOfTestCaseCorrectly()
    {
        var builder = new ApplicationContextBuilder();
        var testCasePath = Path.Combine(
            Environment.CurrentDirectory,
            StructureExplorationIndex.RootFolderName,
            StructureExplorationIndex.TestCasesRelativePaths[6]);

        var pathProvider = new PathProvider();
        pathProvider.UpdatePaths(testCasePath, Constants.SystemTemporaryFolderPath);
        var structureExplorer = GetStructureExplorer(pathProvider);

        var structure = structureExplorer.Explore(builder.WithPath(testCasePath).Build());

        Single(structure.TestCases);

        var testCase = structure.TestCases.First();
        Single(testCase.PreRequestScripts);
        Single(testCase.PostResponseScripts);
    }

    private static TestCaseStructureExplorer GetStructureExplorer(IPathProvider? pathProvider = null)
        => new(pathProvider ?? Substitute.For<IPathProvider>(), Substitute.For<ILogger<TestCaseStructureExplorer>>());
}
