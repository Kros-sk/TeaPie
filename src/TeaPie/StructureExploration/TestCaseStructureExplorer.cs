using Microsoft.Extensions.Logging;

namespace TeaPie.StructureExploration;

internal partial class TestCaseStructureExplorer(ILogger<TestCaseStructureExplorer> logger) : IStructureExplorer
{
    private const string VirtualFolderName = "Virtual";
    private readonly ILogger<TestCaseStructureExplorer> _logger = logger;
    private string? _environmentFileName;
    private string? _initializationScriptName;

    public IReadOnlyCollectionStructure Explore(ApplicationContext applicationContext)
    {
        CheckAndResolveArguments(applicationContext);

        LogStart(applicationContext.Path);

        var collectionStructure = ExploreTestCase(applicationContext);

        LogEnd("     ");

        return collectionStructure;
    }

    #region Helping methods
    private void CheckAndResolveArguments(ApplicationContext applicationContext)
    {
        CheckTestCasePath(applicationContext.Path);
        CheckAndResolveEnvironmentFile(applicationContext.Path, applicationContext.EnvironmentFilePath);
        CheckAndResolveInitializationScript(applicationContext.InitializationScriptPath);
    }

    private static void CheckTestCasePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new InvalidOperationException("Unable to explore test-case structure, if path is empty or missing.");
        }

        if (!System.IO.File.Exists(path))
        {
            throw new InvalidOperationException($"Unable to explore test-case on path '{path}' " +
                "because such a path doesn't exist.");
        }
    }

    private void CheckAndResolveEnvironmentFile(string collectionPath, string environmentFilePath)
        => CheckAndResolveOptionalFile(
            ref _environmentFileName,
            GetEnvironmentFileName(collectionPath),
            environmentFilePath,
            "environment file");

    private void CheckAndResolveInitializationScript(string initializationScriptPath)
        => CheckAndResolveOptionalFile(
            ref _initializationScriptName,
            Constants.DefaultInitializationScriptName + Constants.ScriptFileExtension,
            initializationScriptPath,
            "initialization script");

    private static void CheckAndResolveOptionalFile(
        ref string? fieldToUpdate,
        string updatedValue,
        string filePath,
        string fileName)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            fieldToUpdate = updatedValue;
        }
        else if (!System.IO.File.Exists(filePath))
        {
            throw new InvalidOperationException($"Specified {fileName} on path " +
                $"'{filePath}' does not exist.");
        }
    }

    private static string GetEnvironmentFileName(string path)
        => Path.GetFileNameWithoutExtension(path) + Constants.EnvironmentFileSuffix + Constants.EnvironmentFileExtension;

    private static void InitializeStructure(
        string rootPath,
        string collectionName,
        out Folder rootFolder,
        out CollectionStructure collectionStructure)
    {
        rootFolder = new(rootPath, collectionName, collectionName, null);
        collectionStructure = new CollectionStructure(rootFolder);
        collectionStructure.TryAddFolder(
            new Folder(rootPath, Path.Combine(collectionName, VirtualFolderName), VirtualFolderName, rootFolder));
    }

    private static void UpdateContext(ApplicationContext applicationContext, CollectionStructure collectionStructure)
    {
        if (collectionStructure.HasEnvironmentFile)
        {
            applicationContext.EnvironmentFilePath = collectionStructure.EnvironmentFile.Path;
        }

        if (collectionStructure.HasInitializationScript)
        {
            applicationContext.InitializationScriptPath = collectionStructure.InitializationScript.File.Path;
        }
    }
    #endregion

    #region Exploration methods
    private CollectionStructure ExploreTestCase(ApplicationContext applicationContext)
    {
        var directoryName = Path.GetDirectoryName(applicationContext.Path)
            ?? throw new InvalidOperationException("Unable to explore test-case without parent directory.");

        InitializeStructure(directoryName, directoryName, out var rootFolder, out var collectionStructure);

        Explore(applicationContext.Path, rootFolder, applicationContext, collectionStructure);

        UpdateContext(applicationContext, collectionStructure);
        return collectionStructure;
    }

    private void Explore(string testCasePath, Folder rootFolder, ApplicationContext applicationContext, CollectionStructure collectionStructure)
    {
        ExploreTestCase(testCasePath, rootFolder, collectionStructure);
        // RegisterOptionalFilesIfNeeded(applicationContext, collectionStructure);
    }

    private void ExploreTestCase(string testCasePath, Folder parentFolder, CollectionStructure collectionStructure)
    {
        var files = GetFiles(parentFolder);

        // SearchForOptionalFilesIfNeeded(currentFolder, collectionStructure, files);
        ExploreTestCases(testCasePath, collectionStructure, parentFolder, files);
    }

    private void SearchForOptionalFilesIfNeeded(
        Folder currentFolder,
        CollectionStructure collectionStructure,
        IList<string> files)
    {
        SearchForEnvironmentFileIfNeeded(currentFolder, files, collectionStructure);
        SearchForInitializationScriptIfNeeded(currentFolder, files, collectionStructure);
    }

    private void SearchForEnvironmentFileIfNeeded(
        Folder parentFolder,
        IList<string> files,
        CollectionStructure collectionStructure)
        => SearchForOptionalFileIfNeeded(
            _environmentFileName,
            collectionStructure.HasEnvironmentFile,
            parentFolder,
            files,
            collectionStructure.SetEnvironmentFile);

    private void SearchForInitializationScriptIfNeeded(
        Folder parentFolder,
        IList<string> files,
        CollectionStructure collectionStructure)
        => SearchForOptionalFileIfNeeded(
            _initializationScriptName,
            collectionStructure.HasInitializationScript,
            parentFolder,
            files,
            file => collectionStructure.SetInitializationScript(new Script(file)));

    private static void SearchForOptionalFileIfNeeded(
        string? fileName,
        bool fileExistsInCollection,
        Folder parentFolder,
        IList<string> files,
        Action<File> setFileAction)
    {
        if (fileName is not null && !fileExistsInCollection)
        {
            var foundFile = files.FirstOrDefault(
                f => Path.GetFileName(f).Equals(fileName, StringComparison.OrdinalIgnoreCase));

            if (foundFile is not null)
            {
                setFileAction(File.Create(foundFile, parentFolder));
            }
        }
    }

    private static void ExploreTestCases(
        string testCasePath,
        CollectionStructure collectionStructure,
        Folder currentFolder,
        IList<string> files)
    {
        var preRequestScripts = GetScript(currentFolder, Constants.PreRequestSuffix, files);
        var postResponseScripts = GetScript(currentFolder, Constants.PostResponseSuffix, files);

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
        var subFolderName = Path.GetFileName(subFolderPath.RemoveSlashAtTheEnd());
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

    private void RegisterOptionalFilesIfNeeded(ApplicationContext applicationContext, CollectionStructure collectionStructure)
    {
        RegisterEnvironmentFileIfNeeded(applicationContext.EnvironmentFilePath, collectionStructure);
        RegisterInitializationScriptFileIfNeeded(applicationContext.InitializationScriptPath, collectionStructure);
    }

    private void RegisterEnvironmentFileIfNeeded(string environmentFilePath, CollectionStructure collectionStructure)
        => RegisterOptionalFileIfNeeded(
            _environmentFileName,
            environmentFilePath,
            collectionStructure,
            collectionStructure.SetEnvironmentFile,
            "environment file");

    private void RegisterInitializationScriptFileIfNeeded(
        string initializationScriptPath,
        CollectionStructure collectionStructure)
        => RegisterOptionalFileIfNeeded(
            _initializationScriptName,
            initializationScriptPath,
            collectionStructure,
            file => collectionStructure.SetInitializationScript(new Script(file)),
            "initialization script");

    private static void RegisterOptionalFileIfNeeded(
        string? fileName,
        string filePath,
        CollectionStructure collectionStructure,
        Action<File> setFileAction,
        string fileNameForErrorMessag)
    {
        if (fileName is null)
        {
            if (collectionStructure.TryGetFolder(Path.GetDirectoryName(filePath) ?? string.Empty, out var folder))
            {
                setFileAction(File.Create(filePath, folder));
            }
            else
            {
                throw new InvalidOperationException($"Unable to set {fileNameForErrorMessag} to file outside collection.");
            }
        }
    }
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

    private static IList<string> GetFiles(Folder currentFolder)
        => [.. Directory.GetFiles(currentFolder.Path).OrderBy(path => path, StringComparer.OrdinalIgnoreCase)];

    private static IList<string> GetFolders(Folder currentFolder)
        => [.. Directory.GetDirectories(currentFolder.Path).OrderBy(path => path, StringComparer.OrdinalIgnoreCase)];

    private static Dictionary<string, Script> GetScript(
        Folder folder,
        string desiredSuffix,
        IEnumerable<string> files)
            => files
                .Where(f => Path.GetFileName(f).EndsWith(desiredSuffix + Constants.ScriptFileExtension))
                .Select(file =>
                {
                    var fileName = Path.GetFileName(file);
                    var script = new Script(File.Create(file, folder));

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
    partial void LogStart(string path);

    [LoggerMessage("Test-case explored - {foundArtifacts}.", Level = LogLevel.Information)]
    partial void LogEnd(string foundArtifacts);

    #endregion
}
