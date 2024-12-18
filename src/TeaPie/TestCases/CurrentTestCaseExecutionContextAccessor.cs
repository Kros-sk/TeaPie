using System.Diagnostics;

namespace TeaPie.TestCases;

internal interface ICurrentTestCaseExecutionContextAccessor : IContextAccessor<TestCaseExecutionContext>;

[DebuggerDisplay("{Context}")]
internal class CurrentTestCaseExecutionContextAccessor : ICurrentTestCaseExecutionContextAccessor
{
    public TestCaseExecutionContext? Context { get; set; }
}
