using TeaPie.TestCases;

namespace TeaPie.Testing;

internal interface IRegistrator
{
    void Test(string testName, Action testFunction, bool skipTest = false);
    Task Test(string testName, Func<Task> testFunction, bool skipTest = false);
}

internal class Registrator(ICurrentTestCaseExecutionContextAccessor accessor) : IRegistrator
{
    private readonly ICurrentTestCaseExecutionContextAccessor _testCaseExecutionContextAccessor = accessor;

    public void Test(string testName, Action testFunction, bool skipTest = false)
        => RegisterBase(testName, () => { testFunction(); return Task.CompletedTask; }, skipTest)
            .ConfigureAwait(false).GetAwaiter().GetResult();

    public async Task Test(string testName, Func<Task> testFunction, bool skipTest = false)
        => await RegisterBase(testName, testFunction, skipTest);

    private async Task RegisterBase(string testName, Func<Task> testFunction, bool skipTest = false)
    {
        var testCaseExecutionContext = _testCaseExecutionContextAccessor.Context
            ?? throw new InvalidOperationException("Unable to test if no test-case execution context is provided.");

        var testCase = _testCaseExecutionContextAccessor.Context.TestCase;

        var test = new Test(
            testName,
            skipTest,
            testFunction,
            new TestResult.NotRun() { TestName = testName, TestCasePath = testCase.RequestsFile.RelativePath },
            testCase);

        testCaseExecutionContext.RegisterTest(test);
    }
}
