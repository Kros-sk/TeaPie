using Dunet;

namespace TeaPie.Testing;

[Union]
internal partial record TestResult
{
    public partial record NotRun;
    public partial record Succeed(long Duration);
    public partial record Failed(long Duration, string ErrorMessage, Exception? Exception);
}
