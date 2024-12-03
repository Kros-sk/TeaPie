using System.Diagnostics;

namespace TeaPie.TestCases;

internal interface ITestCaseExecutionContextAccessor
{
    TestCaseExecutionContext? TestCaseExecutionContext { get; set; }
}

[DebuggerDisplay("{TestCaseExecutionContext}")]
internal class TestCaseExecutionContextAccessor : ITestCaseExecutionContextAccessor
{
    public TestCaseExecutionContext? TestCaseExecutionContext { get; set; }
}
