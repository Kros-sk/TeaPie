using System.Diagnostics;
using File = TeaPie.StructureExploration.File;

namespace TeaPie.Http;

[DebuggerDisplay("{RequestFile}")]
internal class RequestExecutionContext(File requestFile)
{
    public File RequestFile { get; set; } = requestFile;
    public string Name { get; set; } = string.Empty;
    public string? RawContent { get; set; }
    public HttpRequestMessage? Request { get; set; }
    public HttpResponseMessage? Response { get; set; }
}
