using TeaPie.Http;
using TeaPie.StructureExploration;
using TeaPie.TestCases;

namespace TeaPie.Tests.Http;

internal static class RequestHelper
{
    public static RequestExecutionContext PrepareRequestContext(string path, bool readFile = true)
    {
        var file = GetFile(path);

        return new RequestExecutionContext(file)
        {
            RawContent = readFile ? System.IO.File.ReadAllText(path) : null,
            ResiliencePipeline = Polly.ResiliencePipeline<HttpResponseMessage>.Empty
        };
    }

    public static TestCaseExecutionContext PrepareTestCaseContext(string path, bool readFile = true)
    {
        var file = GetFile(path);

        var testCase = new TestCase(file);

        return new TestCaseExecutionContext(testCase)
        {
            RequestsFileContent = readFile ? System.IO.File.ReadAllText(path) : null
        };
    }

    public static InternalFile GetFile(string path)
    {
        var folder = new Folder(
            RequestsIndex.RootFolderFullPath,
            RequestsIndex.RootFolderRelativePath,
            RequestsIndex.RootFolderName,
            null);

        return InternalFile.Create(path, folder);
    }
}
