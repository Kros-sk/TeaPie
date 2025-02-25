using TeaPie.Http;

namespace TeaPie.Testing;

internal record PredefinedTestDescription(PredefinedTestType Type, params object[] Parameters)
{
    public RequestExecutionContext? RequestExecutionContext { get; private set; }
    public void SetRequestExecutionContext(RequestExecutionContext requestExecutionContext)
        => RequestExecutionContext = requestExecutionContext;
}
