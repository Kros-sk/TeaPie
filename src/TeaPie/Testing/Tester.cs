using TeaPie.Reporting;
using TeaPie.TestCases;

namespace TeaPie.Testing;

internal class Tester(IReporter reporter) : ITester
{
    private readonly IReporter _reporter = reporter;
    internal TestCaseExecutionContext? TestCaseExecutionContext { get; set; }

    public void Test(string testName, Action testFunction)
        => TestBase(testName, () => { testFunction(); return Task.CompletedTask; })
            .ConfigureAwait(false).GetAwaiter().GetResult();

    public async Task Test(string testName, Func<Task> testFunction)
        => await TestBase(testName, testFunction);

    private async Task TestBase(string testName, Func<Task> testFunction)
    {
        if (TestCaseExecutionContext is null)
        {
            throw new InvalidOperationException("Unable to execute test, if there is no test case assigned.");
        }

        var test = new Test(testName, testFunction);
        TestCaseExecutionContext.TestManager.RegisterTest(test);

        try
        {
            await ExecuteTest(test, testFunction);
        }
        catch (Exception ex)
        {
            TestFailure(test, ex);
        }

        test.Executed = true;
    }

    private void TestFailure(Test test, Exception ex)
    {
        SetTestResult(test.Result, ex);
        _reporter.ReportTestFailure(test.Name, test.Result.Message!);
    }

    private async Task ExecuteTest(Test test, Func<Task> testFunction)
    {
        _reporter.ReportTestStart(test.Name);

        await testFunction();
        test.Result.Success = true;

        _reporter.ReportTestSuccess(test.Name);
    }

    public void AddTestTheory(Action<Theory> testFunction)
    {
        // TODO: Implement
    }

    public void AddTestTheory(string testName, Action<Theory> testFunction)
    {
        // TODO: Implement
    }

    private static void SetTestResult(TestResult testResult, Exception ex)
    {
        testResult.Success = false;
        testResult.Message = ex.Message;
        testResult.StackTrace = ex.StackTrace;
    }
}
