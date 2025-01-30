using Dunet;

namespace TeaPie.Testing;

[Union]
internal partial record TestResult
{
    public partial record NotRun;
    public partial record Passed(string TestName, long Duration);
    public partial record Failed(string TestName, long Duration, string ErrorMessage, Exception? Exception);
}
