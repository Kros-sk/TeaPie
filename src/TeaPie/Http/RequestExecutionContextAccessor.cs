using System.Diagnostics;

namespace TeaPie.Pipelines.Requests;

internal interface IRequestExecutionContextAccessor
{
    RequestExecutionContext? RequestExecutionContext { get; set; }
}

[DebuggerDisplay("{RequestExecutionContext}")]
internal class RequestExecutionContextAccessor : IRequestExecutionContextAccessor
{
    public RequestExecutionContext? RequestExecutionContext { get; set; }
}
