using Dunet;

namespace TeaPie.Testing;

[Union]
internal partial record TestResult
{
    public partial record NotRun;
    public partial record Succeed;
    public partial record Failed(string ErrorMessage, Exception? Exception);
}
