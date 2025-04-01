using Microsoft.Extensions.Logging;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.StructureExploration;

internal partial class TestCaseStructureExplorer(IPathProvider pathProvider, ILogger<TestCaseStructureExplorer> logger)
    : BaseStructureExplorer(pathProvider, logger)
{
    protected override CollectionStructure ExploreStructure(ApplicationContext applicationContext)
    {
        var directoryPath = Path.GetDirectoryName(applicationContext.Path)!;

        InitializeStructure(
            directoryPath,
            Path.GetFileName(directoryPath)!,
            out var rootFolder,
            out var teaPieFolder,
            out var collectionStructure);

        Explore(applicationContext.Path, rootFolder, teaPieFolder, applicationContext, collectionStructure);

        UpdateContext(applicationContext, collectionStructure);

        return collectionStructure;
    }

    #region Exploration

    private void Explore(
        string testCasePath,
        Folder rootFolder,
        Folder teaPieFolder,
        ApplicationContext applicationContext,
        CollectionStructure collectionStructure)
    {
        ExploreTeaPieFolder(teaPieFolder, collectionStructure);
        ExploreTestCase(testCasePath, rootFolder, collectionStructure);

        RegisterOptionalFilesIfNeeded(applicationContext, collectionStructure);
    }

    private void ExploreTestCase(string testCasePath, Folder parentFolder, CollectionStructure collectionStructure)
    {
        var files = GetFiles(parentFolder);

        SearchForOptionalFilesIfNeeded(parentFolder, collectionStructure, files);
        ExploreTestCase(testCasePath, collectionStructure, parentFolder, files);
    }

    #endregion

    #region Validation

    protected override void ValidatePath(string path)
    {
        if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
        {
            throw new InvalidOperationException(
                $"Unable to explore test-case on path '{path}' because such a file doesn't exist.");
        }
    }

    #endregion

    #region Logging

    protected override void LogStart(string path) => LogStartOfProcess(path);

    [LoggerMessage("Exploration of the test-case on path: '{path}' started.", Level = LogLevel.Information)]
    private partial void LogStartOfProcess(string path);

    protected override void LogEnd(CollectionStructure collectionStructure)
    {
        var testCase = collectionStructure.TestCases.First();
        var tokens = new List<string>();

        if (testCase.PreRequestScripts.Any())
        {
            tokens.Add("pre-request script");
        }

        if (testCase.PostResponseScripts.Any())
        {
            tokens.Add("post-response script");
        }

        LogEnd(tokens.Count != 0 ? $"({string.Join(", ", tokens)})" : string.Empty);
    }

    [LoggerMessage("Test-case explored {foundArtifacts}.", Level = LogLevel.Information)]
    private partial void LogEnd(string foundArtifacts);

    #endregion
}
