using TeaPie.Http;
using TeaPie.StructureExploration;
using File = TeaPie.StructureExploration.File;

namespace TeaPie.Tests.Http;

internal static class RequestHelper
{
    public static RequestExecutionContext PrepareContext(string path, bool readFile = true)
    {
        var folder = new Folder(
            RequestsIndex.RootFolderFullPath,
            RequestsIndex.RootFolderRelativePath,
            RequestsIndex.RootFolderName,
            null);

        var file = new File(
            path,
            RequestsIndex.RootFolderFullPath.TrimRootPath(Environment.CurrentDirectory),
            Path.GetFileName(path),
            folder);

        return new RequestExecutionContext(file)
        {
            RawContent = readFile ? System.IO.File.ReadAllText(path) : null
        };
    }
}
