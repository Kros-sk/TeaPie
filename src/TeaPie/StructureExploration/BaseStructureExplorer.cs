using Microsoft.Extensions.Logging;

namespace TeaPie.StructureExploration;

internal abstract class BaseStructureExplorer(ILogger logger) : IStructureExplorer
{
    protected const string RemoteFolderName = "Remote";
    protected string _remoteFolderPath = string.Empty;

    protected readonly ILogger _logger = logger;
    protected string? _environmentFileName;
    protected string? _initializationScriptName;

    public IReadOnlyCollectionStructure Explore(ApplicationContext applicationContext)
    {
        CheckAndResolveArguments(applicationContext);

        LogStart(applicationContext.Path);

        var collectionStructure = ExploreStructure(applicationContext);

        LogEnd(collectionStructure);

        return collectionStructure;
    }

    protected void InitializeStructure(
        string rootPath, string collectionName, out Folder rootFolder, out CollectionStructure collectionStructure)
    {
        rootFolder = new(rootPath, collectionName, collectionName, null);
        collectionStructure = new CollectionStructure(rootFolder);

        RegisterRemoteFolder(rootPath, collectionName, rootFolder, collectionStructure);
    }

    protected static void UpdateContext(ApplicationContext applicationContext, CollectionStructure collectionStructure)
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

    #region Exploration

    protected static void ExploreTestCase(
        string testCasePath,
        CollectionStructure collectionStructure,
        Folder currentFolder,
        IList<string> files)
    {
        var testCaseName = Path.GetFileName(testCasePath);
        var preRequestScript = GetScript(testCaseName, currentFolder, Constants.PreRequestSuffix, files);
        var postResponseScript = GetScript(testCaseName, currentFolder, Constants.PostResponseSuffix, files);

        var testCase = GetTestCase(currentFolder, out _, out _, out _, testCasePath);

        testCase.PreRequestScripts = preRequestScript is not null ? [preRequestScript] : [];
        testCase.PostResponseScripts = postResponseScript is not null ? [postResponseScript] : [];

        if (!collectionStructure.TryAddTestCase(testCase))
        {
            throw new InvalidOperationException($"Unable to register same test-case twice. {testCase.RequestsFile.Path}");
        }
    }

    protected abstract CollectionStructure ExploreStructure(ApplicationContext applicationContext);

    #endregion

    #region Validation

    protected abstract void ValidatePath(string path);

    protected void CheckAndResolveArguments(ApplicationContext applicationContext)
    {
        ValidatePath(applicationContext.Path);
        CheckAndResolveEnvironmentFile(applicationContext.Path, applicationContext.EnvironmentFilePath);
        CheckAndResolveInitializationScript(applicationContext.InitializationScriptPath);
    }

    protected void CheckAndResolveEnvironmentFile(string path, string environmentFilePath)
        => CheckAndResolveOptionalFile(
            ref _environmentFileName,
            GetEnvironmentFileName(path),
            environmentFilePath,
            "environment file");

    protected void CheckAndResolveInitializationScript(string initializationScriptPath)
        => CheckAndResolveOptionalFile(
            ref _initializationScriptName,
            Constants.DefaultInitializationScriptName + Constants.ScriptFileExtension,
            initializationScriptPath,
            "initialization script");

    protected static void CheckAndResolveOptionalFile(
        ref string? fieldToUpdate, string updatedValue, string filePath, string fileName)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            fieldToUpdate = updatedValue;
        }
        else if (!System.IO.File.Exists(filePath))
        {
            throw new InvalidOperationException($"Specified {fileName} on path '{filePath}' does not exist.");
        }
    }

    #endregion

    #region Getter Methods

    protected static IList<string> GetFiles(Folder currentFolder)
        => [.. Directory.GetFiles(currentFolder.Path).OrderBy(path => path, StringComparer.OrdinalIgnoreCase)];

    protected static Script? GetScript(
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

    protected static TestCase GetTestCase(
        Folder currentFolder,
        out string fileName,
        out string relativePath,
        out File requestFileObj,
        string reqFile)
    {
        fileName = Path.GetFileName(reqFile);
        relativePath = GetRelativePath(currentFolder, fileName);
        requestFileObj = new(reqFile, relativePath, fileName, currentFolder);

        return new TestCase(requestFileObj);
    }

    protected static string GetRelativePath(Folder parentFolder, string folderName)
        => Path.Combine(parentFolder.RelativePath, folderName);

    protected static string GetEnvironmentFileName(string path)
        => Path.GetFileNameWithoutExtension(path) + Constants.EnvironmentFileSuffix + Constants.EnvironmentFileExtension;

    #endregion

    #region Search Methods

    protected void SearchForOptionalFilesIfNeeded(
        Folder currentFolder, CollectionStructure collectionStructure, IList<string> files)
    {
        SearchForEnvironmentFileIfNeeded(currentFolder, files, collectionStructure);
        SearchForInitializationScriptIfNeeded(currentFolder, files, collectionStructure);
    }

    protected void SearchForEnvironmentFileIfNeeded(
        Folder parentFolder, IList<string> files, CollectionStructure collectionStructure)
        => SearchForOptionalFileIfNeeded(
            _environmentFileName,
            collectionStructure.HasEnvironmentFile,
            parentFolder,
            files,
            collectionStructure.SetEnvironmentFile);

    protected void SearchForInitializationScriptIfNeeded(
        Folder parentFolder, IList<string> files, CollectionStructure collectionStructure)
        => SearchForOptionalFileIfNeeded(
            _initializationScriptName,
            collectionStructure.HasInitializationScript,
            parentFolder,
            files,
            file => collectionStructure.SetInitializationScript(new Script(file)));

    protected static void SearchForOptionalFileIfNeeded(
        string? fileName,
        bool fileExistsInCollection,
        Folder parentFolder,
        IList<string> files,
        Action<File> setFileAction)
    {
        if (fileName is not null && !fileExistsInCollection)
        {
            var foundFile = files.FirstOrDefault(f => Path.GetFileName(f).Equals(fileName, StringComparison.OrdinalIgnoreCase));

            if (foundFile is not null)
            {
                setFileAction(File.Create(foundFile, parentFolder));
            }
        }
    }

    #endregion

    #region Registration Methods

    protected static Folder RegisterFolder(Folder currentFolder, CollectionStructure collectionStructure, string subFolderPath)
    {
        var subFolderName = Path.GetFileName(subFolderPath.RemoveSlashAtTheEnd());
        Folder subFolder = new(subFolderPath, GetRelativePath(currentFolder, subFolderName), subFolderName, currentFolder);

        collectionStructure.TryAddFolder(subFolder);

        return subFolder;
    }
    protected void RegisterRemoteFolder(
        string rootPath, string collectionName, Folder rootFolder, CollectionStructure collectionStructure)
    {
        _remoteFolderPath = Path.Combine(rootPath, RemoteFolderName);
        collectionStructure.TryAddFolder(
            new Folder(_remoteFolderPath, Path.Combine(collectionName, RemoteFolderName), RemoteFolderName, rootFolder));
    }


    protected void RegisterOptionalFilesIfNeeded(ApplicationContext applicationContext, CollectionStructure collectionStructure)
    {
        RegisterEnvironmentFileIfNeeded(applicationContext.EnvironmentFilePath, collectionStructure);
        RegisterInitializationScriptFileIfNeeded(applicationContext.InitializationScriptPath, collectionStructure);
    }

    protected void RegisterEnvironmentFileIfNeeded(string environmentFilePath, CollectionStructure collectionStructure)
        => RegisterOptionalFileIfNeeded(
            _environmentFileName,
            environmentFilePath,
            collectionStructure,
            collectionStructure.SetEnvironmentFile,
            "environment file");

    protected void RegisterInitializationScriptFileIfNeeded(
        string initializationScriptPath,
        CollectionStructure collectionStructure)
        => RegisterOptionalFileIfNeeded(
            _initializationScriptName,
            initializationScriptPath,
            collectionStructure,
            file => collectionStructure.SetInitializationScript(new Script(file)),
            "initialization script");

    protected void RegisterOptionalFileIfNeeded(
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

    #region Logging

    protected abstract void LogStart(string path);

    protected abstract void LogEnd(CollectionStructure structure);

    #endregion
}
