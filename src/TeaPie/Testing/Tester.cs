using System.Diagnostics;
using TeaPie.Reporting;
using TeaPie.TestCases;

namespace TeaPie.Testing;

internal class Tester(IReporter reporter, ICurrentTestCaseExecutionContextAccessor accessor) : ITester
{
    private readonly IReporter _reporter = reporter;
    private readonly ICurrentTestCaseExecutionContextAccessor _testCaseExecutionContextAccessor = accessor;
    private readonly Stopwatch _stopWatch = new();

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
        _stopWatch.Reset();

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
        _stopWatch.Stop();

        test = test with { Result = new TestResult.Failed(_stopWatch.ElapsedMilliseconds, ex.Message, ex) };

        _reporter.ReportTestFailure(test.Name, ex.Message, _stopWatch.ElapsedMilliseconds);
        return test;
    }

    private async Task<Test> ExecuteTest(Test test, Func<Task> testFunction, TestCaseExecutionContext testCaseExecutionContext)
    {
        _reporter.ReportTestStart(test.Name, testCaseExecutionContext.TestCase.RequestsFile.RelativePath);

        _stopWatch.Start();

        await testFunction();

        _stopWatch.Stop();

        test = test with { Result = new TestResult.Succeed(_stopWatch.ElapsedMilliseconds) };

        _reporter.ReportTestSuccess(test.Name, _stopWatch.ElapsedMilliseconds);
        return test;
    }
    #endregion
}
