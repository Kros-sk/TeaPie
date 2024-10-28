using Microsoft.Extensions.Logging;
using TeaPie.Helpers;
using TeaPie.StructureExploration.Records;
using File = TeaPie.StructureExploration.Records.File;

namespace TeaPie.StructureExploration;

internal interface IStructureExplorer
{
    IReadOnlyDictionary<string, TestCase> ExploreCollectionStructure(string rootPath);
}

internal class StructureExplorer(ILogger<StructureExplorer> logger) : IStructureExplorer
{
    private readonly ILogger<StructureExplorer> _logger = logger;

    public IReadOnlyDictionary<string, TestCase> ExploreCollectionStructure(string rootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath, "Collection path");

        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFoundException("Provided folder doesn't exist.");
        }

        _logger.LogInformation("Exploration of the collection started on path: '{path}'.", rootPath);

        var testCases = new Dictionary<string, TestCase>();
        var folderName = Path.GetFileName(rootPath.TrimEnd(Path.DirectorySeparatorChar));

        Folder rootFolder = new(rootPath, folderName, folderName, null);
        ExploreFolder(rootFolder, testCases);

        _logger.LogInformation("Collection explored, found {count} test cases.", testCases.Count);

        return testCases;
    }

    /// <summary>
    /// Recursive Depth-first algorithm, which examines file system tree. Traversal path is saved in <param cref="testCases">
    /// parameter in form of test cases. Each folder can have sub-folders and/or test cases. Test case is represented by '.http'
    /// file and possibly by other (e.g. script files with '.csx' extension) files.
    /// </summary>
    /// <param name="currentFolder">Folder that should be examined.</param>
    /// <param name="testCases">Depth-first order of test cases.</param>
    private static void ExploreFolder(Folder currentFolder, Dictionary<string, TestCase> testCases)
    {
        var subFolderPaths = Directory.GetDirectories(currentFolder.Path)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);

        var files = Directory.GetFiles(currentFolder.Path).Order()
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);

        foreach (var subFolderPath in subFolderPaths)
        {
            var subFolderName = Path.GetFileName(subFolderPath.TrimEnd(Path.DirectorySeparatorChar));
            Folder subFolder = new(subFolderPath, GetRelativePath(currentFolder, subFolderName), subFolderName, currentFolder);

            ExploreFolder(subFolder, testCases);
        }

        ExploreTestCases(testCases, currentFolder, files);
    }

    private static void ExploreTestCases(
        Dictionary<string, TestCase> testCases,
        Folder currentFolder,
        IEnumerable<string> files)
    {
        string fileName, relativePath;
        File requestFileObj;
        TestCase testCase;

        foreach (var reqFile in files.Where(f => f.EndsWith(Constants.RequestFileExtension)).Order().ToList())
        {
            fileName = Path.GetFileName(reqFile);
            relativePath = $"{currentFolder.RelativePath}{Path.DirectorySeparatorChar}{fileName}";
            requestFileObj = new(reqFile, relativePath, fileName, currentFolder);

            testCase = new TestCase(requestFileObj)
            {
                PreRequestScripts = GetScripts(currentFolder, reqFile, Constants.PreRequestSuffix, files),
                PostResponseScripts = GetScripts(currentFolder, reqFile, Constants.PostResponseSuffix, files)
            };

            testCases[reqFile] = testCase;
        }
    }

    private static IEnumerable<Script> GetScripts(
        Folder folder,
        string requestFileName,
        string desiredSuffix,
        IEnumerable<string> files)
            => files
                .Where(f =>
                    Path.GetFileName(f).EndsWith(desiredSuffix + Constants.ScriptFileExtension) &&
                    Path.GetFileNameWithoutExtension(f)
                        .StartsWith(Path.GetFileNameWithoutExtension(requestFileName).TrimSuffix(Constants.RequestSuffix)))
                .Select(file =>
                {
                    var fileName = Path.GetFileName(file);
                    return new Script(new File(
                        file,
                        $"{folder.RelativePath}{Path.DirectorySeparatorChar}{fileName}",
                        fileName,
                        folder));
                })
                .Order();

    private static string GetRelativePath(Folder parentFolder, string folderName)
        => $"{parentFolder.RelativePath}{Path.DirectorySeparatorChar}{folderName}";
}
