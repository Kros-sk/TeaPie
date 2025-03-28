using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.StructureExploration;
using TeaPie.StructureExploration.Paths;
using static Xunit.Assert;

namespace TeaPie.Tests.StructureExploration;

public class TestCaseStructureExplorerShould
{
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
        var structureExplorer = GetStructureExplorer();
        var builder = new ApplicationContextBuilder();
        var collectionPath = Path.Combine(Environment.CurrentDirectory, StructureExplorationIndex.CollectionFolderRelativePath);

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

        var structureExplorer = GetStructureExplorer();

        Single(structureExplorer.Explore(builder.WithPath(testCasePath).Build()).TestCases);
    }

    [Fact]
    public void CreateRemoteFolder()
    {
        var builder = new ApplicationContextBuilder();
        var testCasePath = Path.Combine(
            Environment.CurrentDirectory,
            StructureExplorationIndex.RootFolderName,
            StructureExplorationIndex.TestCasesRelativePaths[6]);

        var structureExplorer = GetStructureExplorer();

        var structure = structureExplorer.Explore(builder.WithPath(testCasePath).Build());
        var remoteFolderPath = Path.Combine(
            Environment.CurrentDirectory,
            StructureExplorationIndex.CollectionFolderRelativePath,
            BaseStructureExplorer.RemoteFolderName);

        True(structure.TryGetFolder(remoteFolderPath, out var folder));
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

        var structureExplorer = GetStructureExplorer();

        var structure = structureExplorer.Explore(builder.WithPath(testCasePath).Build());

        Single(structure.TestCases);

        var testCase = structure.TestCases.First();
        Single(testCase.PreRequestScripts);
        Single(testCase.PostResponseScripts);
    }

    private static TestCaseStructureExplorer GetStructureExplorer()
        => new(Substitute.For<IPathProvider>(), Substitute.For<ILogger<TestCaseStructureExplorer>>());
}
