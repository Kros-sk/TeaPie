using Microsoft.Extensions.Logging;

namespace TeaPie.StructureExploration;

internal interface IStructureExplorer
{
    IReadOnlyCollectionStructure ExploreCollectionStructure(string rootPath);
}

internal partial class StructureExplorer(ILogger<StructureExplorer> logger) : IStructureExplorer
{
    private readonly ILogger<StructureExplorer> _logger = logger;

    public IReadOnlyCollectionStructure ExploreCollectionStructure(string rootPath)
    {
        CheckArgument(rootPath);

        LogStartOfCollectionExploration(rootPath);

        InitializeStructure(rootPath, out var rootFolder, out var collectionStructure);

        Explore(rootFolder, collectionStructure);

        LogEndOfCollectionExploration(collectionStructure.TestCases.Count);

        return collectionStructure;
    }

    #region Helping methods
    private static void CheckArgument(string rootPath)
    {
        if (string.IsNullOrEmpty(rootPath))
        {
            throw new InvalidOperationException("Unable to explore collection structure, if path is empty or missing.");
        }

        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFoundException($"Provided folder doesn't exist. ({rootPath})");
        }
    }

    private static void InitializeStructure(string rootPath, out Folder rootFolder, out CollectionStructure collectionStructure)
    {
        var folderName = Path.GetFileName(rootPath.TrimEnd(Path.DirectorySeparatorChar));
        rootFolder = new(rootPath, folderName, folderName, null);
        collectionStructure = new CollectionStructure(rootFolder);
    }
    #endregion

    #region Exploration methods
    private static void Explore(Folder rootFolder, CollectionStructure collectionStructure)
        => ExploreFolder(rootFolder, collectionStructure);

    /// <summary>
    /// Recursive depth-first algorithm, which examines file system tree. Whole structure is gradually formed within
    /// <paramref name="collectionStructure"/> parameter in form of folders and test-cases. Each folder can have sub-folders
    /// and/or test cases. Test-case is represented by '.http' file and possibly by other files (e.g. script '.csx' files).
    /// </summary>
    /// <param name="currentFolder">Folder to be explored.</param>
    /// <param name="collectionStructure">List of explored test-cases.</param>
    private static void ExploreFolder(Folder currentFolder, CollectionStructure collectionStructure)
    {
        var subFolderPaths = GetFolders(currentFolder);
        var files = GetFiles(currentFolder);

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
        IEnumerable<string> files)
    {
        var preRequestScripts = GetScripts(currentFolder, Constants.PreRequestSuffix, files);
        var postResponseScripts = GetScripts(currentFolder, Constants.PostResponseSuffix, files);

        foreach (var reqFile in files.Where(f => f.EndsWith(Constants.RequestFileExtension)).Order())
        {
            var testCase = GetTestCase(currentFolder, out var fileName, out var relativePath, out var requestFileObj, reqFile);

            RegisterPreRequestScript(preRequestScripts, testCase, fileName);
            RegisterPostResponseScript(postResponseScripts, testCase, fileName);

            if (!collectionStructure.TryAddTestCase(testCase))
            {
                throw new InvalidOperationException($"Unable to register same test-case twice. {testCase.RequestsFile.Path}");
            }
        }
    }
    #endregion

    #region Registration methods
    private static Folder RegisterFolder(Folder currentFolder, CollectionStructure collectionStructure, string subFolderPath)
    {
        var subFolderName = Path.GetFileName(subFolderPath.TrimEnd(Path.DirectorySeparatorChar));
        Folder subFolder = new(subFolderPath, GetRelativePath(currentFolder, subFolderName), subFolderName, currentFolder);

        collectionStructure.TryAddFolder(subFolder);

        return subFolder;
    }

    private static void RegisterPreRequestScript(
        Dictionary<string, Script> preRequestScripts,
        TestCase testCase,
        string fileName)
        => RegisterScript(preRequestScripts, out testCase.PreRequestScripts, Constants.PreRequestSuffix, fileName);

    private static void RegisterPostResponseScript(
        Dictionary<string, Script> postResponseScripts,
        TestCase testCase,
        string fileName)
        => RegisterScript(postResponseScripts, out testCase.PostResponseScripts, Constants.PostResponseSuffix, fileName);

    private static void RegisterScript(
        Dictionary<string, Script> sourceScriptCollection,
        out IEnumerable<Script> targetScriptCollection,
        string scriptSuffix,
        string fileName)
        => targetScriptCollection = sourceScriptCollection
            .TryGetValue(GetRelatedScriptFileName(fileName, scriptSuffix), out var script)
                ? [script]
                : [];
    #endregion

    #region Getting methods
    private static TestCase GetTestCase(
        Folder currentFolder,
        out string fileName,
        out string relativePath,
        out File requestFileObj,
        string reqFile)
    {
        fileName = Path.GetFileName(reqFile);
        relativePath = $"{currentFolder.RelativePath}{Path.DirectorySeparatorChar}{fileName}";
        requestFileObj = new(reqFile, relativePath, fileName, currentFolder);

        return new TestCase(requestFileObj);
    }

    private static IOrderedEnumerable<string> GetFiles(Folder currentFolder)
    => Directory.GetFiles(currentFolder.Path).Order()
         .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);

    private static IOrderedEnumerable<string> GetFolders(Folder currentFolder)
        => Directory.GetDirectories(currentFolder.Path)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);

    private static Dictionary<string, Script> GetScripts(
        Folder folder,
        string desiredSuffix,
        IEnumerable<string> files)
            => files
                .Where(f => Path.GetFileName(f).EndsWith(desiredSuffix + Constants.ScriptFileExtension))
                .Select(file =>
                {
                    var fileName = Path.GetFileName(file);
                    var script = new Script(new File(
                        file,
                        $"{folder.RelativePath}{Path.DirectorySeparatorChar}{fileName}",
                        fileName,
                        folder));

                    return new KeyValuePair<string, Script>(fileName, script);
                })
                .ToDictionary();

    private static string GetRelatedScriptFileName(string requestFileName, string desiredSuffix)
        => Path.GetFileNameWithoutExtension(requestFileName).TrimSuffix(Constants.RequestSuffix) +
            desiredSuffix + Constants.ScriptFileExtension;

    private static string GetRelativePath(Folder parentFolder, string folderName)
        => $"{parentFolder.RelativePath}{Path.DirectorySeparatorChar}{folderName}";
    #endregion

    #region Logging
    [LoggerMessage("Exploration of the collection started on path: '{path}'.", Level = LogLevel.Information)]
    partial void LogStartOfCollectionExploration(string path);

    [LoggerMessage("Collection explored, found {countOfTestCases} test cases.", Level = LogLevel.Information)]
    partial void LogEndOfCollectionExploration(int countOfTestCases);
    #endregion
}
