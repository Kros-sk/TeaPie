using System.Diagnostics;

namespace TeaPie.TestCases;

internal interface ICurrentTestCaseExecutionContextAccessor
{
    TestCaseExecutionContext? CurrentTestCaseExecutionContext { get; set; }
}

[DebuggerDisplay("{CurrentTestCaseExecutionContext}")]
internal class CurrentTestCaseExecutionContextAccessor : ICurrentTestCaseExecutionContextAccessor
{
    public TestCaseExecutionContext? CurrentTestCaseExecutionContext { get; set; }
}
