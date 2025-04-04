using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TeaPie.Logging;
using TeaPie.Reporting;
using TeaPie.StructureExploration;
using TeaPie.TestCases;

namespace TeaPie.Testing;

internal partial class Tester(
    ICurrentTestCaseExecutionContextAccessor accessor,
    ITestResultsSummaryReporter resultsSummaryReporter,
    ILogger<Tester> logger) : ITester
{
    private readonly ILogger<Tester> _logger = logger;
    private readonly ICurrentTestCaseExecutionContextAccessor _testCaseExecutionContextAccessor = accessor;
    private readonly ITestResultsSummaryReporter _resultsSummaryReporter = resultsSummaryReporter;
    private readonly Stopwatch _stopWatch = new();

    private bool _hasExecutedAnyTest;

    #region Determined tests
    public void Test(string testName, Action testFunction, bool skipTest = false)
        => TestBase(testName, () => { testFunction(); return Task.CompletedTask; }, skipTest)
            .ConfigureAwait(false).GetAwaiter().GetResult();

    public async Task Test(string testName, Func<Task> testFunction, bool skipTest = false)
        => await TestBase(testName, testFunction, skipTest);

    private async Task TestBase(string testName, Func<Task> testFunction, bool skipTest = false)
    {
        var testCaseExecutionContext = _testCaseExecutionContextAccessor.Context
            ?? throw new InvalidOperationException("Unable to test if no test-case execution context is provided.");

        StartRunIfFirstTest();
        var testCase = _testCaseExecutionContextAccessor.Context.TestCase;

        var test = new Test(
            testName,
            testFunction,
            new TestResult.NotRun() { TestName = testName, TestCasePath = testCase.RequestsFile.RelativePath },
            testCase);

        test = await ExecuteOrSkipTest(testName, skipTest, testCaseExecutionContext, test);

        testCaseExecutionContext.RegisterTest(test);
    }

    private void StartRunIfFirstTest()
    {
        if (!_hasExecutedAnyTest)
        {
            _resultsSummaryReporter.Initialize();
            _hasExecutedAnyTest = true;
        }
    }

    private async Task<Test> ExecuteOrSkipTest(
        string testName, bool skipTest, TestCaseExecutionContext testCaseExecutionContext, Test test)
    {
        if (skipTest)
        {
            LogTestSkip(testName, testCaseExecutionContext.TestCase.RequestsFile.RelativePath);
            _resultsSummaryReporter.RegisterTestResult(testCaseExecutionContext.TestCase.Name, test.Result);
        }
        else
        {
            test = await ExecuteTest(test, testCaseExecutionContext.TestCase);
        }

        return test;
    }

    private async Task<Test> ExecuteTest(Test test, TestCase? testCase)
    {
        _stopWatch.Reset();

        try
        {
            return await ExecuteTest(test, test.Function, testCase);
        }
        catch (Exception ex)
        {
            return TestFailure(test, ex, testCase);
        }
    }

    private Test TestFailure(Test test, Exception ex, TestCase? testCase)
    {
        _stopWatch.Stop();

        var result = new TestResult.Failed(_stopWatch.ElapsedMilliseconds, ex.Message, ex)
        {
            TestName = test.Name,
            TestCasePath = testCase?.RequestsFile.RelativePath ?? string.Empty
        };
        test = test with { Result = result };
        _resultsSummaryReporter.RegisterTestResult(testCase?.Name ?? string.Empty, result);

        LogTestFailure(test.Name, ex.Message, _stopWatch.ElapsedMilliseconds);
        return test;
    }

    private void LogTestFailure(string name, string message, long elapsedMilliseconds)
    {
        LogTestFailureLine(name, elapsedMilliseconds.ToHumanReadableTime());
        LogTestFailureReason(message);
    }

    private async Task<Test> ExecuteTest(Test test, Func<Task> testFunction, TestCase? testCase)
    {
        LogTestStart(test.Name, testCase?.RequestsFile.RelativePath);

        _stopWatch.Start();

        await testFunction();

        _stopWatch.Stop();

        var result = new TestResult.Passed(_stopWatch.ElapsedMilliseconds)
        {
            TestName = test.Name,
            TestCasePath = testCase?.RequestsFile.RelativePath ?? string.Empty
        };
        test = test with { Result = result };
        _resultsSummaryReporter.RegisterTestResult(testCase?.Name ?? string.Empty, result);

        LogTestSuccess(test.Name, _stopWatch.ElapsedMilliseconds.ToHumanReadableTime());
        return test;
    }
    #endregion

    [LoggerMessage(Message = "Skipping test: '{Name}' ({Path})", Level = LogLevel.Information)]
    partial void LogTestSkip(string Name, string Path);

    [LoggerMessage(Message = "Running test: '{Name}' ({Path})", Level = LogLevel.Information)]
    partial void LogTestStart(string Name, string? Path);

    [LoggerMessage(Message = "Test Passed: '{Name}' in {Duration}", Level = LogLevel.Information)]
    partial void LogTestSuccess(string Name, string Duration);

    [LoggerMessage(Message = "Test '{Name}' failed: after {Duration}", Level = LogLevel.Error)]
    partial void LogTestFailureLine(string Name, string Duration);

    [LoggerMessage(Message = "Reason: {Reason}", Level = LogLevel.Error)]
    partial void LogTestFailureReason(string Reason);
}
