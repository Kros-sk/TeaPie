using Polly;
using System.Diagnostics;
using TeaPie.TestCases;
using File = TeaPie.StructureExploration.File;

namespace TeaPie.Http;

[DebuggerDisplay("{RequestFile}")]
internal class RequestExecutionContext(File requestFile, TestCaseExecutionContext? testCaseExecutionContext = null)
{
    public TestCaseExecutionContext? TestCaseExecutionContext { get; set; } = testCaseExecutionContext;
    public File RequestFile { get; set; } = requestFile;
    public string Name { get; set; } = string.Empty;
    public string? RawContent { get; set; }
    public HttpRequestMessage? Request { get; set; }
    public HttpResponseMessage? Response { get; set; }
    public ResiliencePipeline? ResiliencePipeline { get; set; }
}
