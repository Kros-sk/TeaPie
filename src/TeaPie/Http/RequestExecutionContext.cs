using Polly;
using System.Diagnostics;
using TeaPie.Http.Auth;
using TeaPie.StructureExploration;
using TeaPie.TestCases;

namespace TeaPie.Http;

[DebuggerDisplay("{RequestFile}")]
internal class RequestExecutionContext(InternalFile requestFile, TestCaseExecutionContext? testCaseExecutionContext = null)
{
    public TestCaseExecutionContext? TestCaseExecutionContext { get; set; } = testCaseExecutionContext;
    public InternalFile RequestFile { get; set; } = requestFile;
    public string Name { get; set; } = string.Empty;
    public string? RawContent { get; set; }
    public HttpRequestMessage? Request { get; set; }
    public HttpResponseMessage? Response { get; set; }
    public ResiliencePipeline<HttpResponseMessage>? ResiliencePipeline { get; set; }
    public IAuthProvider? AuthProvider { get; set; }
}
