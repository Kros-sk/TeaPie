using File = TeaPie.StructureExploration.IO.File;

namespace TeaPie.Pipelines.Requests;

internal class RequestExecutionContext(File request)
{
    public File RequestFile { get; set; } = request;
    public HttpRequestMessage? Request { get; set; }
    public HttpResponseMessage? Response { get; set; }
    public string? RawContent { get; set; }
}
