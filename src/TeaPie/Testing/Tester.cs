using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TeaPie.Logging;
using TeaPie.Reporting;
using TeaPie.StructureExploration;
using TeaPie.TestCases;

namespace TeaPie.Testing;

internal interface ITester
{
    Task<Test> ExecuteOrSkipTest(Test test, TestCase? testCase);
    bool ExecuteTestSync(string testName, HttpResponseMessage response);
}

internal partial class Tester(
    ICurrentTestCaseExecutionContextAccessor accessor,
    ITestResultsSummaryReporter resultsSummaryReporter,
    ILogger<Tester> logger) : ITester
{
    private readonly Stopwatch _stopWatch = new();

    public async Task<Test> ExecuteOrSkipTest(Test test, TestCase? testCase)
    {
        _stopWatch.Restart();

        if (test.SkipTest)
        {
            LogTestSkip(logger, test.Name, _stopWatch.ElapsedMilliseconds.ToHumanReadableTime());
            resultsSummaryReporter.RegisterTestResult(testCase?.Name ?? string.Empty, test.Result);
            return test;
        }

        if (test.Result is not TestResult.NotRun)
        {
            LogTestAlreadyExecuted(logger, test.Name, _stopWatch.ElapsedMilliseconds.ToHumanReadableTime());
            resultsSummaryReporter.RegisterTestResult(testCase?.Name ?? string.Empty, test.Result);
            return test;
        }

        try
        {
            return await ExecuteTestInternal(test, test.Function, testCase);
        }
        catch (Exception ex)
        {
            return HandleTestFailure(test, ex, testCase);
        }
    }

    public bool ExecuteTestSync(string testName, HttpResponseMessage response)
    {
        var testCaseExecutionContext = accessor.Context!;
        var test = testCaseExecutionContext.GetTest(testName)
            ?? throw new InvalidOperationException($"Test \"{testName}\" doesn't exists.");

        var previousResponse = testCaseExecutionContext.Response;
        testCaseExecutionContext.RegisterResponse(response);
        _stopWatch.Restart();

        try
        {
            test.Function().GetAwaiter().GetResult();
            _stopWatch.Stop();

            var result = CreatePassedResult(test, testCase: testCaseExecutionContext.TestCase);
            testCaseExecutionContext.UpdateTest(test with { Result = result });

            LogTestPassedDuringRetry(logger, testName, _stopWatch.ElapsedMilliseconds.ToHumanReadableTime());
            return true;
        }
        catch (Exception ex)
        {
            _stopWatch.Stop();

            var result = CreateFailedResult(test, ex, testCase: testCaseExecutionContext.TestCase);
            testCaseExecutionContext.UpdateTest(test with { Result = result });

            LogTestFailedDuringRetry(logger, testName, ex.Message);
            return false;
        }
        finally
        {
            if (previousResponse != null)
            {
                testCaseExecutionContext.RegisterResponse(previousResponse);
            }
        }
    }

    private Test HandleTestFailure(Test test, Exception ex, TestCase? testCase)
    {
        _stopWatch.Stop();

        var result = CreateFailedResult(test, ex, testCase);
        var updatedTest = test with { Result = result };

        resultsSummaryReporter.RegisterTestResult(testCase?.Name ?? string.Empty, result);
        LogTestFailure(test.Name, ex.Message, _stopWatch.ElapsedMilliseconds);

        return updatedTest;
    }

    private void LogTestFailure(string name, string message, long elapsedMilliseconds)
    {
        LogTestFailureLine(logger, name, elapsedMilliseconds.ToHumanReadableTime());
        LogTestFailureReason(logger, message);
    }

    private async Task<Test> ExecuteTestInternal(Test test, Func<Task> testFunction, TestCase? testCase)
    {
        _stopWatch.Start();
        await testFunction();
        _stopWatch.Stop();

        var result = CreatePassedResult(test, testCase);
        var updatedTest = test with { Result = result };

        resultsSummaryReporter.RegisterTestResult(testCase?.Name ?? string.Empty, result);
        LogTestSuccess(logger, test.Name, _stopWatch.ElapsedMilliseconds.ToHumanReadableTime());

        return updatedTest;
    }

    private TestResult.Passed CreatePassedResult(Test test, TestCase? testCase) =>
        new(_stopWatch.ElapsedMilliseconds)
        {
            TestName = test.Name,
            TestCasePath = testCase?.RequestsFile.RelativePath ?? string.Empty
        };

    private TestResult.Failed CreateFailedResult(Test test, Exception ex, TestCase? testCase) =>
        new(_stopWatch.ElapsedMilliseconds, ex.Message, ex)
        {
            TestName = test.Name,
            TestCasePath = testCase?.RequestsFile.RelativePath ?? string.Empty
        };

    #region Logging

    [LoggerMessage(Message = "Test Passed: '{Name}' in {Duration}", Level = LogLevel.Information)]
    private static partial void LogTestSuccess(ILogger logger, string Name, string Duration);

    [LoggerMessage(Message = "Test '{Name}' failed: after {Duration}", Level = LogLevel.Error)]
    private static partial void LogTestFailureLine(ILogger logger, string Name, string Duration);

    [LoggerMessage(Message = "Reason: {Reason}", Level = LogLevel.Error)]
    private static partial void LogTestFailureReason(ILogger logger, string Reason);

    [LoggerMessage(Message = "Test passed during retry evaluation: '{TestName}' in {Duration}", Level = LogLevel.Information)]
    private static partial void LogTestPassedDuringRetry(ILogger logger, string TestName, string Duration);

    [LoggerMessage(Message = "Test '{TestName}' failed during retry evaluation: {Message}", Level = LogLevel.Error)]
    private static partial void LogTestFailedDuringRetry(ILogger logger, string TestName, string Message);

    [LoggerMessage(Message = "Skipping test: '{Name}' ({Path})", Level = LogLevel.Information)]
    private static partial void LogTestSkip(ILogger logger, string Name, string Path);

    [LoggerMessage(Message = "Test '{Name}' ({Path}) was already executed during retry evaluation", Level = LogLevel.Information)]
    private static partial void LogTestAlreadyExecuted(ILogger logger, string Name, string Path);

    #endregion
}
