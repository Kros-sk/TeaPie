using Microsoft.Extensions.Logging;
using TeaPie.StructureExploration.Paths;
using TeaPie.TestCases;

namespace TeaPie.StructureExploration;

internal partial class TestCaseStructureExplorer(
    IPathProvider pathProvider, ILogger<TestCaseStructureExplorer> logger, TpFileParser tpFileParser)
    : BaseStructureExplorer(pathProvider, logger, tpFileParser)
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

        if (testCasePath.IsTpFile())
        {
            ExploreTpFile(testCasePath, collectionStructure, rootFolder);
        }
        else
        {
            ExploreTestCase(testCasePath, rootFolder, collectionStructure);
        }

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
                $"Unable to explore test case at path '{path}' because such a file doesn't exist.");
        }
    }

    #endregion

    #region Logging

    protected override void LogStart(string path) => LogStartOfProcess(path);

    [LoggerMessage("Exploration of the test case at path: '{path}' started.", Level = LogLevel.Information)]
    private partial void LogStartOfProcess(string path);

    protected override void LogEnd(CollectionStructure collectionStructure, string duration)
    {
        var testCases = collectionStructure.TestCases;
        if (testCases.Count > 1)
        {
            LogEnd(duration, $"({testCases.Count} test cases from .tp file)");
            return;
        }

        var testCase = testCases.First();
        var hasPreRequest = HasPreRequestScript(testCase);
        var hasPostResponse = HasPostResponseScript(testCase);

        var tokens = new List<string>();
        if (hasPreRequest)
        {
            tokens.Add("pre-request script");
        }

        if (hasPostResponse)
        {
            tokens.Add("post-response script");
        }

        if (tokens.Count == 0)
        {
            tokens.Add("HTTP only");
        }

        LogEnd(duration, $"({string.Join(", ", tokens)})");
    }

    private static bool HasPreRequestScript(TestCase testCase)
        => testCase.IsFromTpFile
            ? !string.IsNullOrWhiteSpace(testCase.TpDefinition?.InitContent)
            : testCase.PreRequestScripts.Any();

    private static bool HasPostResponseScript(TestCase testCase)
        => testCase.IsFromTpFile
            ? !string.IsNullOrWhiteSpace(testCase.TpDefinition?.TestContent)
            : testCase.PostResponseScripts.Any();

    [LoggerMessage("The test case explored in {duration} {foundArtifacts}.", Level = LogLevel.Information)]
    private partial void LogEnd(string duration, string foundArtifacts);

    #endregion
}
