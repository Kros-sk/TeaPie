using TeaPie.TestCases;

namespace TeaPie.Testing;

internal interface ITester : ITesterExposer
{
    void SetCurrentTestCaseExecutionContext(TestCaseExecutionContext? testCaseExecutionContext);
}
