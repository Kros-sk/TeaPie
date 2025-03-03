using TeaPie.Http;

namespace TeaPie.Testing;

internal record TestDescription(string Directive, IReadOnlyDictionary<string, string> Parameters)
{
    public RequestExecutionContext? RequestExecutionContext { get; private set; }
    public void SetRequestExecutionContext(RequestExecutionContext requestExecutionContext)
        => RequestExecutionContext = requestExecutionContext;
}
