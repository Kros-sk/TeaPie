using System.Diagnostics;

namespace TeaPie.Http;

internal interface IRequestExecutionContextAccessor : IContextAccessor<RequestExecutionContext>;

[DebuggerDisplay("{Context}")]
internal class RequestExecutionContextAccessor : IRequestExecutionContextAccessor
{
    public RequestExecutionContext? Context { get; set; }
}
