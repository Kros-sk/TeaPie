using File = TeaPie.StructureExploration.IO.File;

namespace TeaPie.Pipelines.Requests;

internal class RequestExecutionContext(File request)
{
    public File Request { get; set; } = request;
    public HttpRequestMessage? RequestMessage { get; set; }
    public string? RawContent { get; set; }
}
