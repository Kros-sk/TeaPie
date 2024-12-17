using TeaPie.TestCases;

namespace TeaPie.Testing;

internal interface ITester
{
    void Test(string testName, Action testFunction);

    Task Test(string testName, Func<Task> testFunction);

    void SetCurrentTestCaseExecutionContext(TestCaseExecutionContext? testCaseExecutionContext);
}
