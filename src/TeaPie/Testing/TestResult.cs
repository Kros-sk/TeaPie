using Dunet;

namespace TeaPie.Testing;

[Union]
internal partial record TestResult
{
    public partial record NotRun;
    public partial record Passed(long Duration);
    public partial record Failed(long Duration, string ErrorMessage, Exception? Exception);

    public required string TestName { get; init; }
}
