using TeaPie.Extensions;
using TeaPie.Pipelines.Requests;
using TeaPie.StructureExploration.IO;
using File = TeaPie.StructureExploration.IO.File;

namespace TeaPie.Tests.Requests;

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
