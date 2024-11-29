using System.Diagnostics;

namespace TeaPie.Http;

internal interface IRequestExecutionContextAccessor
{
    RequestExecutionContext? RequestExecutionContext { get; set; }
}

[DebuggerDisplay("{RequestExecutionContext}")]
internal class RequestExecutionContextAccessor : IRequestExecutionContextAccessor
{
    public RequestExecutionContext? RequestExecutionContext { get; set; }
}
