using Microsoft.Extensions.Logging;

namespace TeaPie.StructureExploration;

internal interface IStructureExplorer
{
    IReadOnlyCollectionStructure ExploreCollectionStructure(ApplicationContext applicationContext);
}

internal partial class StructureExplorer(ILogger<StructureExplorer> logger) : IStructureExplorer
{
    private readonly ILogger<StructureExplorer> _logger = logger;
    private string? _environmentFileName;

    public IReadOnlyCollectionStructure ExploreCollectionStructure(ApplicationContext applicationContext)
    {
        CheckArguments(applicationContext);

        LogStartOfCollectionExploration(applicationContext.Path);

        InitializeStructure(applicationContext.Path, out var rootFolder, out var collectionStructure);

        Explore(rootFolder, applicationContext.EnvironmentFilePath, collectionStructure);

        UpdateContext(applicationContext, collectionStructure);

        LogEndOfCollectionExploration(collectionStructure.TestCases.Count);

        return collectionStructure;
    }

    #region Helping methods
    private void CheckArguments(ApplicationContext applicationContext)
    {
        if (string.IsNullOrEmpty(applicationContext.Path))
        {
            throw new InvalidOperationException("Unable to explore collection structure, if path is empty or missing.");
        }

        if (!Directory.Exists(applicationContext.Path))
        {
            throw new DirectoryNotFoundException($"Provided folder on path '{applicationContext.Path}' doesn't exist.");
        }

        if (string.IsNullOrEmpty(applicationContext.EnvironmentFilePath))
        {
            _environmentFileName = GetEnvironmentFileName(applicationContext.Path);
        }
        else if (!System.IO.File.Exists(applicationContext.EnvironmentFilePath))
        {
            throw new InvalidOperationException($"Specified environment file on path " +
                $"'{applicationContext.EnvironmentFilePath}' does not exist.");
        }
    }

    private static string GetEnvironmentFileName(string path)
        => Path.GetFileNameWithoutExtension(path) + Constants.EnvironmentFileSuffix + Constants.EnvironmentFileExtension;

    private static void InitializeStructure(string rootPath, out Folder rootFolder, out CollectionStructure collectionStructure)
    {
        var folderName = Path.GetFileName(rootPath.TrimEnd(Path.DirectorySeparatorChar));
        rootFolder = new(rootPath, folderName, folderName, null);
        collectionStructure = new CollectionStructure(rootFolder);
    }

    private static void UpdateContext(ApplicationContext applicationContext, CollectionStructure collectionStructure)
    {
        if (collectionStructure.HasEnvironmentFile)
        {
            applicationContext.EnvironmentFilePath = collectionStructure.EnvironmentFile.Path;
        }
    }
    #endregion

    #region Exploration methods
    private void Explore(Folder rootFolder, string environmentFilePath, CollectionStructure collectionStructure)
    {
        ExploreFolder(rootFolder, collectionStructure);
        RegisterEnvironmentFileIfNeeded(environmentFilePath, collectionStructure);
    }

    /// <summary>
    /// Recursive depth-first algorithm, which examines file system tree. Whole structure is gradually formed within
    /// <paramref name="collectionStructure"/> parameter in form of folders and test-cases. Each folder can have sub-folders
    /// and/or test cases. Test-case is represented by <b>'.http'</b> file and possibly by other files
    /// (e.g. script <b>'.csx'</b> files).
    /// </summary>
    /// <param name="currentFolder">Folder to be explored.</param>
    /// <param name="collectionStructure">List of explored test-cases.</param>
    private void ExploreFolder(Folder currentFolder, CollectionStructure collectionStructure)
    {
        var subFolderPaths = GetFolders(currentFolder);
        var files = GetFiles(currentFolder);

        SearchForEnvironmentFileIfNeeded(currentFolder, files, collectionStructure);

        foreach (var subFolderPath in subFolderPaths)
        {
            var subFolder = RegisterFolder(currentFolder, collectionStructure, subFolderPath);
            ExploreFolder(subFolder, collectionStructure);
        }

        ExploreTestCases(collectionStructure, currentFolder, files);
    }

    private void SearchForEnvironmentFileIfNeeded(
        Folder parentFolder,
        IOrderedEnumerable<string> files,
        CollectionStructure collectionStructure)
    {
        if (_environmentFileName is not null && !collectionStructure.HasEnvironmentFile)
        {
            var envFile = files.FirstOrDefault(f => Path.GetFileName(f).Equals(_environmentFileName));
            if (envFile is not null)
            {
                collectionStructure.SetEnvironmentFile(GetFile(envFile, parentFolder));
            }
        }
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

    private void RegisterEnvironmentFileIfNeeded(string environmentFilePath, CollectionStructure collectionStructure)
    {
        if (_environmentFileName is null)
        {
            var folder = collectionStructure.Folders.FirstOrDefault(x => x.Path.Equals(Path.GetDirectoryName(environmentFilePath)));
            if (folder is null)
            {
                throw new InvalidOperationException("Unable to set environment file to file outside collection.");
            }
            else
            {
                collectionStructure.SetEnvironmentFile(GetFile(environmentFilePath, folder));
            }
        }
    }
    #endregion

    #region Getting methods
    private static File GetFile(string filePath, Folder folder)
    {
        var fileName = Path.GetFileName(filePath);
        return new File(
            filePath,
            $"{folder.RelativePath}{Path.DirectorySeparatorChar}{fileName}",
            fileName,
            folder);
    }

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
                    var script = new Script(GetFile(file, folder));

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
