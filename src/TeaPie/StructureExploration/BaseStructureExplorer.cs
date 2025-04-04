using Microsoft.Extensions.Logging;
using TeaPie.Logging;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.StructureExploration;

internal abstract class BaseStructureExplorer(IPathProvider pathProvider, ILogger logger) : IStructureExplorer
{
    public const string RemoteFolderName = "~Remote";
    protected string _remoteFolderPath = string.Empty;

    protected readonly ILogger _logger = logger;
    protected string? _environmentFileName;
    protected string? _initializationScriptName;
    protected IPathProvider _pathProvider = pathProvider;

    public IReadOnlyCollectionStructure Explore(ApplicationContext applicationContext)
    {
        CheckAndResolveArguments(applicationContext);

        LogStart(applicationContext.Path);

        long elapsedTime = 0;
        var collectionStructure = Logging.Timer.Execute(
            () => ExploreStructure(applicationContext),
            realTime => elapsedTime = realTime);

        LogEnd(collectionStructure, elapsedTime.ToHumanReadableTime());

        return collectionStructure;
    }

    protected void InitializeStructure(
        string rootPath,
        string collectionName,
        out Folder rootFolder,
        out Folder teaPieFolder,
        out CollectionStructure collectionStructure)
    {
        rootFolder = new(rootPath, collectionName, collectionName, null);
        collectionStructure = new CollectionStructure(rootFolder);

        teaPieFolder = RegisterTeaPieFolder(collectionName, rootFolder, collectionStructure);
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

    protected void ExploreTeaPieFolder(Folder teaPieFolder, CollectionStructure collectionStructure)
    {
        var files = GetFiles(teaPieFolder);
        SearchForOptionalFilesIfNeeded(teaPieFolder, collectionStructure, files);
    }

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
            throw new InvalidOperationException($"Unable to register same test case twice. {testCase.RequestsFile.Path}");
        }
    }

    protected abstract CollectionStructure ExploreStructure(ApplicationContext applicationContext);

    #endregion

    #region Validation

    protected abstract void ValidatePath(string path);

    protected void CheckAndResolveArguments(ApplicationContext applicationContext)
    {
        ValidatePath(applicationContext.Path);
        CheckAndResolveEnvironmentFile(applicationContext.EnvironmentFilePath);
        CheckAndResolveInitializationScript(applicationContext.InitializationScriptPath);
    }

    protected void CheckAndResolveEnvironmentFile(string environmentFilePath)
        => CheckAndResolveOptionalFile(
            ref _environmentFileName,
            Constants.DefaultEnvironmentFileName + Constants.EnvironmentFileExtension,
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
            throw new InvalidOperationException($"Specified {fileName} at path '{filePath}' does not exist.");
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
        return file is not null ? new Script(InternalFile.Create(file, folder)) : null;
    }

    private static string GetRelatedScriptFileName(string requestFileName, string desiredSuffix)
        => Path.GetFileNameWithoutExtension(requestFileName).TrimSuffix(Constants.RequestSuffix) +
            desiredSuffix + Constants.ScriptFileExtension;

    protected static TestCase GetTestCase(
        Folder currentFolder,
        out string fileName,
        out string relativePath,
        out InternalFile requestFileObj,
        string reqFile)
    {
        fileName = Path.GetFileName(reqFile);
        relativePath = GetRelativePath(currentFolder, fileName);
        requestFileObj = new(reqFile, relativePath, currentFolder);

        return new TestCase(requestFileObj);
    }

    protected static string GetRelativePath(Folder parentFolder, string folderName)
        => Path.Combine(parentFolder.RelativePath, folderName);

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

    protected void SearchForOptionalFileIfNeeded(
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
                var file = GetFile(foundFile, parentFolder);
                setFileAction(file);
            }
        }
    }

    private File GetFile(string foundFile, Folder parentFolder)
        => File.BelongsTo(foundFile, _pathProvider.RootPath)
            ? InternalFile.Create(foundFile, parentFolder)
            : GetExternalFile(foundFile);

    private ExternalFile GetExternalFile(string foundFile)
    {
        if (File.BelongsTo(foundFile, _pathProvider.TeaPieFolderPath))
        {
            return new ExternalFile(foundFile)
            {
                RelativePath =
                    Path.Combine(_pathProvider.StructureName, Constants.TeaPieFolderName, Path.GetFileName(foundFile))
            };
        }
        else
        {
            return new ExternalFile(foundFile);
        }
    }

    #endregion

    #region Registration Methods

    protected static Folder RegisterFolder(Folder currentFolder, CollectionStructure collectionStructure, string subFolderPath)
    {
        var subFolderName = Path.GetFileName(subFolderPath.TrimSlashAtTheEnd());
        Folder subFolder = new(subFolderPath, GetRelativePath(currentFolder, subFolderName), subFolderName, currentFolder);

        collectionStructure.TryAddFolder(subFolder);

        return subFolder;
    }

    protected Folder RegisterTeaPieFolder(
        string collectionName, Folder rootFolder, CollectionStructure collectionStructure)
    {
        var teaPieFolder =
            new Folder(
                _pathProvider.TeaPieFolderPath,
                Path.Combine(collectionName, Constants.TeaPieFolderName),
                Constants.TeaPieFolderName,
                rootFolder);

        collectionStructure.TryAddFolder(teaPieFolder);
        return teaPieFolder;
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
        Action<InternalFile> setFileAction,
        string fileNameForErrorMessage)
    {
        if (fileName is null)
        {
            if (!collectionStructure.TryGetFolder(filePath, out var folder) &&
                !collectionStructure.TryGetFolder(_pathProvider.TeaPieFolderPath, out folder))
            {
                throw new InvalidOperationException($"Unable to find parent folder of {fileNameForErrorMessage}.");
            }

            setFileAction(InternalFile.Create(filePath, folder));
        }
    }

    #endregion

    #region Logging

    protected abstract void LogStart(string path);

    protected abstract void LogEnd(CollectionStructure structure, string duration);

    #endregion
}
