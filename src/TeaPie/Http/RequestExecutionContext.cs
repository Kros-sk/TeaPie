using System.Diagnostics;
using File = TeaPie.StructureExploration.IO.File;

namespace TeaPie.Pipelines.Requests;

[DebuggerDisplay("{RequestFile}")]
internal class RequestExecutionContext(File request)
{
    public File RequestFile { get; set; } = request;
    public string? RawContent { get; set; }
    public HttpRequestMessage? Request { get; set; }
    public HttpResponseMessage? Response { get; set; }
}
