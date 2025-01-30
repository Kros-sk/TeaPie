using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TeaPie.Logging;
using TeaPie.Reporting;
using TeaPie.TestCases;

namespace TeaPie.Testing;

internal partial class Tester(
    ICurrentTestCaseExecutionContextAccessor accessor,
    ITestsResultsSummaryReporter resultsSummaryReporter,
    ILogger<Tester> logger) : ITester
{
    private readonly ILogger<Tester> _logger = logger;
    private readonly ICurrentTestCaseExecutionContextAccessor _testCaseExecutionContextAccessor = accessor;
    private readonly ITestsResultsSummaryReporter _resultsSummaryReporter = resultsSummaryReporter;
    private readonly Stopwatch _stopWatch = new();

    #region Determined tests
    public void Test(string testName, Action testFunction)
        => TestBase(testName, () => { testFunction(); return Task.CompletedTask; })
            .ConfigureAwait(false).GetAwaiter().GetResult();

    public async Task Test(string testName, Func<Task> testFunction)
        => await TestBase(testName, testFunction);

    private async Task TestBase(string testName, Func<Task> testFunction)
    {
        var testCaseExecutionContext = _testCaseExecutionContextAccessor.Context
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

        var result = new TestResult.Failed(test.Name, _stopWatch.ElapsedMilliseconds, ex.Message, ex);
        test = test with { Result = result };
        _resultsSummaryReporter.RegisterTestResult(result);

        LogTestFailure(test.Name, ex.Message, _stopWatch.ElapsedMilliseconds);
        return test;
    }

    private void LogTestFailure(string name, string message, long elapsedMilliseconds)
    {
        LogTestFailureLine(name, elapsedMilliseconds.ToHumanReadableTime());
        LogTestFailureReason(message);
    }

    private async Task<Test> ExecuteTest(Test test, Func<Task> testFunction, TestCaseExecutionContext testCaseExecutionContext)
    {
        LogTestStart(test.Name, testCaseExecutionContext.TestCase.RequestsFile.RelativePath);

        _stopWatch.Start();

        await testFunction();

        _stopWatch.Stop();

        var result = new TestResult.Passed(test.Name, _stopWatch.ElapsedMilliseconds);
        test = test with { Result = result };
        _resultsSummaryReporter.RegisterTestResult(result);

        LogTestSuccess(test.Name, _stopWatch.ElapsedMilliseconds.ToHumanReadableTime());
        return test;
    }
    #endregion

    [LoggerMessage(Message = "Running test: '{Name}' ({Path})", Level = LogLevel.Information)]
    partial void LogTestStart(string Name, string Path);

    [LoggerMessage(Message = "Test Passed: '{Name}' in {Duration}", Level = LogLevel.Information)]
    partial void LogTestSuccess(string Name, string Duration);

    [LoggerMessage(Message = "Test '{Name}' failed: after {Duration}", Level = LogLevel.Error)]
    partial void LogTestFailureLine(string Name, string Duration);

    [LoggerMessage(Message = "Reason: {Reason}", Level = LogLevel.Error)]
    partial void LogTestFailureReason(string Reason);
}
