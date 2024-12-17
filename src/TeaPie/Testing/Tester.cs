using TeaPie.Reporting;
using TeaPie.TestCases;

namespace TeaPie.Testing;

internal class Tester(IReporter reporter, ICurrentTestCaseExecutionContextAccessor accessor) : ITester
{
    private readonly IReporter _reporter = reporter;
    private readonly ICurrentTestCaseExecutionContextAccessor _testCaseExecutionContextAccessor = accessor;

    #region Tests
    public void Test(string testName, Action testFunction)
        => TestBase(testName, () => { testFunction(); return Task.CompletedTask; })
            .ConfigureAwait(false).GetAwaiter().GetResult();

    public async Task Test(string testName, Func<Task> testFunction)
        => await TestBase(testName, testFunction);

    private async Task TestBase(string testName, Func<Task> testFunction)
    {
        var testCaseExecutionContext = _testCaseExecutionContextAccessor.CurrentTestCaseExecutionContext
            ?? throw new InvalidOperationException("Unable to test if no test case execution context is provided.");

        var test = new Test(testName, testFunction, new TestResult.NotRun());

        test = await ExecuteTest(test, testCaseExecutionContext);

        testCaseExecutionContext.RegisterTest(test);
    }

    private async Task<Test> ExecuteTest(Test test, TestCaseExecutionContext testCaseExecutionContext)
    {
        try
        {
            return await ExecuteTest(test, test.Function, testCaseExecutionContext);
        }
        catch (Exception ex)
        {
            return TestFailure(test, ex);
        }
    }

    private Test TestFailure(Test test, Exception ex)
    {
        test = test with { Result = new TestResult.Failed(ex.Message, ex) };
        _reporter.ReportTestFailure(test.Name, ex.Message);
        return test;
    }

    private async Task<Test> ExecuteTest(Test test, Func<Task> testFunction, TestCaseExecutionContext testCaseExecutionContext)
    {
        _reporter.ReportTestStart(test.Name, testCaseExecutionContext.TestCase.RequestsFile.RelativePath);

        await testFunction();
        test = test with { Result = new TestResult.Succeed() };

        _reporter.ReportTestSuccess(test.Name);
        return test;
    }
    #endregion
}
