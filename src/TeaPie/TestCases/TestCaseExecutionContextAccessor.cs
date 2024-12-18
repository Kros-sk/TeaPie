using System.Diagnostics;

namespace TeaPie.TestCases;

internal interface ITestCaseExecutionContextAccessor : IContextAccessor<TestCaseExecutionContext>;

[DebuggerDisplay("{Context}")]
internal class TestCaseExecutionContextAccessor : ITestCaseExecutionContextAccessor
{
    public TestCaseExecutionContext? Context { get; set; }
}
