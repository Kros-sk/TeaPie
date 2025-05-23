﻿using Microsoft.Extensions.Logging;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.StructureExploration;

internal partial class CollectionStructureExplorer(IPathProvider pathProvider, ILogger<CollectionStructureExplorer> logger)
    : BaseStructureExplorer(pathProvider, logger)
{
    protected override CollectionStructure ExploreStructure(ApplicationContext applicationContext)
    {
        InitializeStructure(
            applicationContext.Path,
            applicationContext.StructureName,
            out var collectionFolder,
            out var teaPieFolder,
            out var collectionStructure);

        Explore(collectionFolder, teaPieFolder, applicationContext, collectionStructure);

        UpdateContext(applicationContext, collectionStructure);

        return collectionStructure;
    }

    #region Exploration

    private void Explore(
        Folder collectionFolder,
        Folder teaPieFolder,
        ApplicationContext applicationContext,
        CollectionStructure collectionStructure)
    {
        ExploreTeaPieFolder(teaPieFolder, collectionStructure);
        ExploreFolder(collectionFolder, collectionStructure);

        RegisterOptionalFilesIfNeeded(applicationContext, collectionStructure);
    }

    /// <summary>
    /// Recursive depth-first algorithm, which examines file system tree. Whole structure is gradually formed within
    /// <paramref name="collectionStructure"/> parameter in form of folders and test cases. Each folder can have sub-folders
    /// and/or test cases. Test case is represented by <b>'.http'</b> file and possibly by other files
    /// (e.g. script <b>'.csx'</b> files).
    /// </summary>
    /// <param name="currentFolder">Folder to be explored.</param>
    /// <param name="collectionStructure">List of explored test cases.</param>
    private void ExploreFolder(Folder currentFolder, CollectionStructure collectionStructure)
    {
        var subFolderPaths = GetFolders(currentFolder);
        var files = GetFiles(currentFolder);

        SearchForOptionalFilesIfNeeded(currentFolder, collectionStructure, files);

        foreach (var subFolderPath in subFolderPaths)
        {
            var subFolder = RegisterFolder(currentFolder, collectionStructure, subFolderPath);
            ExploreFolder(subFolder, collectionStructure);
        }

        ExploreTestCases(collectionStructure, currentFolder, files);
    }

    private static void ExploreTestCases(
        CollectionStructure collectionStructure,
        Folder currentFolder,
        IList<string> files)
    {
        foreach (var reqFile in files.Where(f => f.EndsWith(Constants.RequestFileExtension)).Order())
        {
            var testCase = GetTestCase(currentFolder, out var fileName, out var relativePath, out var requestFileObj, reqFile);

            ExploreTestCase(testCase.RequestsFile.Path, collectionStructure, currentFolder, files);
        }
    }

    #endregion

    #region Validation

    protected override void ValidatePath(string path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            throw new InvalidOperationException(
                $"Unable to explore collection at path '{path}' because such a path doesn't exist.");
        }
    }

    #endregion

    #region Getter Methods

    private static IList<string> GetFolders(Folder currentFolder)
        => [.. Directory.GetDirectories(currentFolder.Path).OrderBy(path => path, StringComparer.OrdinalIgnoreCase)];

    #endregion

    #region Logging

    protected override void LogStart(string path) => LogStartOfExploration(path);

    protected override void LogEnd(CollectionStructure structure, string duration)
        => LogEnd(duration, structure.TestCases.Count);

    [LoggerMessage("Exploration of the collection started at path: '{path}'.", Level = LogLevel.Information)]
    partial void LogStartOfExploration(string path);

    [LoggerMessage("Collection explored in {duration}, found {countOfTestCases} test cases.", Level = LogLevel.Information)]
    partial void LogEnd(string duration, int countOfTestCases);

    #endregion
}
