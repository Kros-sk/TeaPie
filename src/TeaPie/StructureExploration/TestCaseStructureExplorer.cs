using Microsoft.Extensions.Logging;

namespace TeaPie.StructureExploration;

internal partial class TestCaseStructureExplorer(ILogger<TestCaseStructureExplorer> logger) : IStructureExplorer
{
    private const string RemoteFolderName = "Remote";
    private string _remoteFolderPath = string.Empty;

    private readonly ILogger<TestCaseStructureExplorer> _logger = logger;
    private string? _environmentFileName;
    private string? _initializationScriptName;

    public IReadOnlyCollectionStructure Explore(ApplicationContext applicationContext)
    {
        CheckAndResolveArguments(applicationContext);

        LogStart(applicationContext.Path);

        var collectionStructure = ExploreTestCase(applicationContext);

        LogEnd(collectionStructure);

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
                "because such a file doesn't exist.");
        }

        var directoryPath = Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException("Unable to explore test-case without parent directory.");

        _ = Path.GetFileName(directoryPath)
            ?? throw new InvalidOperationException("Unable to explore test-case without parent directory.");
    }

    private void CheckAndResolveEnvironmentFile(string testCasePath, string environmentFilePath)
        => CheckAndResolveOptionalFile(
            ref _environmentFileName,
            GetEnvironmentFileName(testCasePath),
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
        => Path.GetFileNameWithoutExtension(path).TrimSuffix(Constants.RequestSuffix) +
        Constants.EnvironmentFileSuffix + Constants.EnvironmentFileExtension;

    private void InitializeStructure(
        string rootPath,
        string collectionName,
        out Folder rootFolder,
        out CollectionStructure collectionStructure)
    {
        rootFolder = new(rootPath, collectionName, collectionName, null);
        collectionStructure = new CollectionStructure(rootFolder);
        RegisterRemoteFolder(rootPath, collectionName, rootFolder, collectionStructure);
    }

    private void RegisterRemoteFolder(
        string rootPath, string collectionName, Folder rootFolder, CollectionStructure collectionStructure)
    {
        _remoteFolderPath = Path.Combine(rootPath, RemoteFolderName);
        RegisterFolder(rootFolder, collectionStructure, _remoteFolderPath);
        collectionStructure.TryAddFolder(
            new Folder(
                _remoteFolderPath,
                Path.Combine(collectionName, RemoteFolderName),
                RemoteFolderName,
                rootFolder)
        );
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
        var directoryPath = Path.GetDirectoryName(applicationContext.Path)!;
        var directoryName = Path.GetFileName(directoryPath)!;

        InitializeStructure(directoryPath, directoryName, out var rootFolder, out var collectionStructure);

        Explore(applicationContext.Path, rootFolder, applicationContext, collectionStructure);

        UpdateContext(applicationContext, collectionStructure);
        return collectionStructure;
    }

    private void Explore(
        string testCasePath, Folder rootFolder, ApplicationContext applicationContext, CollectionStructure collectionStructure)
    {
        ExploreTestCase(testCasePath, rootFolder, collectionStructure);
        RegisterOptionalFilesIfNeeded(applicationContext, collectionStructure);
    }

    private void ExploreTestCase(string testCasePath, Folder parentFolder, CollectionStructure collectionStructure)
    {
        var files = GetFiles(parentFolder);

        SearchForOptionalFilesIfNeeded(parentFolder, collectionStructure, files);
        ExploreTestCase(testCasePath, collectionStructure, parentFolder, files);
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

    private static void ExploreTestCase(
        string testCasePath,
        CollectionStructure collectionStructure,
        Folder currentFolder,
        IList<string> files)
    {
        var testCaseName = Path.GetFileName(testCasePath);
        var preRequestScript = GetScript(testCaseName, currentFolder, Constants.PreRequestSuffix, files);
        var postResponseScript = GetScript(testCaseName, currentFolder, Constants.PostResponseSuffix, files);

        var testCase = GetTestCase(currentFolder, testCasePath);

        testCase.PreRequestScripts = preRequestScript is not null ? [preRequestScript] : [];
        testCase.PostResponseScripts = postResponseScript is not null ? [postResponseScript] : [];

        if (!collectionStructure.TryAddTestCase(testCase))
        {
            throw new InvalidOperationException($"Unable to register same test-case twice. {testCase.RequestsFile.Path}");
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

    private void RegisterOptionalFileIfNeeded(
        string? fileName,
        string filePath,
        CollectionStructure collectionStructure,
        Action<File> setFileAction,
        string fileNameForErrorMessage)
    {
        if (fileName is null)
        {
            if (!collectionStructure.TryGetFolder(filePath, out var folder) &&
                !collectionStructure.TryGetFolder(_remoteFolderPath, out folder))
            {
                throw new InvalidOperationException($"Unable to find parent folder of {fileNameForErrorMessage}.");
            }

            setFileAction(File.Create(filePath, folder));
        }
    }
    #endregion

    #region Getting methods
    private static TestCase GetTestCase(Folder currentFolder, string reqFilePath)
    {
        var fileName = Path.GetFileName(reqFilePath);
        var relativePath = GetRelativePath(currentFolder, fileName);
        var requestFileObj = new File(reqFilePath, relativePath, fileName, currentFolder);

        return new TestCase(requestFileObj);
    }

    private static IList<string> GetFiles(Folder currentFolder)
        => [.. Directory.GetFiles(currentFolder.Path).OrderBy(path => path, StringComparer.OrdinalIgnoreCase)];

    private static Script? GetScript(
        string requestFileName,
        Folder folder,
        string desiredSuffix,
        IEnumerable<string> files)
    {
        var file = files.FirstOrDefault(
            f => Path.GetFileName(f).Equals(GetRelatedScriptFileName(requestFileName, desiredSuffix)));
        return file is not null ? new Script(File.Create(file, folder)) : null;
    }

    private static string GetRelatedScriptFileName(string requestFileName, string desiredSuffix)
        => Path.GetFileNameWithoutExtension(requestFileName).TrimSuffix(Constants.RequestSuffix) +
            desiredSuffix + Constants.ScriptFileExtension;

    private static string GetRelativePath(Folder parentFolder, string folderName)
        => Path.Combine(parentFolder.RelativePath, folderName);

    #endregion

    #region Logging

    [LoggerMessage("Exploration of the test-case on path: '{path}' started.", Level = LogLevel.Information)]
    partial void LogStart(string path);

    private void LogEnd(CollectionStructure collectionStructure)
    {
        var testCase = collectionStructure.TestCases.First();
        var tokens = new List<string>();

        if (testCase.PreRequestScripts.Any())
        {
            tokens.Add(Constants.PreRequestSuffix.TrimStart('-'));
        }

        if (testCase.PostResponseScripts.Any())
        {
            tokens.Add(Constants.PostResponseSuffix.TrimStart('-'));
        }

        LogEnd(tokens.Count != 0 ? $"({string.Join(", ", tokens)})" : string.Empty);
    }

    [LoggerMessage("Test-case explored {foundArtifacts}.", Level = LogLevel.Information)]
    partial void LogEnd(string foundArtifacts);

    #endregion
}
