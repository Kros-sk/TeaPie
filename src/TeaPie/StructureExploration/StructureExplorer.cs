using Microsoft.Extensions.Logging;
using TeaPie.Extensions;
using TeaPie.StructureExploration.IO;
using File = TeaPie.StructureExploration.IO.File;

namespace TeaPie.StructureExploration;

internal interface IStructureExplorer
{
    IReadOnlyDictionary<string, TestCase> ExploreCollectionStructure(string rootPath);
}

internal partial class StructureExplorer(ILogger<StructureExplorer> logger) : IStructureExplorer
{
    private readonly ILogger<StructureExplorer> _logger = logger;

    public IReadOnlyDictionary<string, TestCase> ExploreCollectionStructure(string rootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath, "Collection path");

        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFoundException("Provided folder doesn't exist.");
        }

        LogStartOfCollectionExploration(rootPath);

        var testCases = new Dictionary<string, TestCase>();
        var folderName = Path.GetFileName(rootPath.TrimEnd(Path.DirectorySeparatorChar));

        Folder rootFolder = new(rootPath, folderName, folderName, null);
        ExploreFolder(rootFolder, testCases);

        LogEndOfCollectionExploration(testCases.Count);

        return testCases;
    }

    /// <summary>
    /// Recursive Depth-first algorithm, which examines file system tree. Traversal path is saved in <paramref name="testCases"/>
    /// parameter in form of test cases. Each folder can have sub-folders and/or test cases. Test case is represented by '.http'
    /// file and possibly by other (e.g. script files with '.csx' extension) files.
    /// </summary>
    /// <param name="currentFolder">Folder to be explored.</param>
    /// <param name="testCases">List of explored test cases.</param>
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

        var preRequestScripts = GetScripts(currentFolder, Constants.PreRequestSuffix, files);
        var postResponseScripts = GetScripts(currentFolder, Constants.PostResponseSuffix, files);

        foreach (var reqFile in files.Where(f => f.EndsWith(Constants.RequestFileExtension)).Order())
        {
            fileName = Path.GetFileName(reqFile);
            relativePath = $"{currentFolder.RelativePath}{Path.DirectorySeparatorChar}{fileName}";
            requestFileObj = new(reqFile, relativePath, fileName, currentFolder);

            testCase = new TestCase(requestFileObj);

            if (preRequestScripts.TryGetValue(GetRelatedScriptFileName(fileName, Constants.PreRequestSuffix), out var preReqScript))
            {
                testCase.PreRequestScripts = [preReqScript];
            }

            if (postResponseScripts.TryGetValue(GetRelatedScriptFileName(fileName, Constants.PostResponseSuffix), out var postResScript))
            {
                testCase.PostResponseScripts = [postResScript];
            }

            testCases[reqFile] = testCase;
        }
    }

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

    [LoggerMessage("Exploration of the collection started on path: '{path}'.", Level = LogLevel.Information)]
    partial void LogStartOfCollectionExploration(string path);

    [LoggerMessage("Collection explored, found {countOfTestCases} test cases.", Level = LogLevel.Information)]
    partial void LogEndOfCollectionExploration(int countOfTestCases);
}
