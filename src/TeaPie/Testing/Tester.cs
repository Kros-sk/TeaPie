using System.Diagnostics.CodeAnalysis;
using TeaPie.Reporting;
using TeaPie.TestCases;

namespace TeaPie.Testing;

internal class Tester(IReporter reporter) : ITester
{
    private readonly IReporter _reporter = reporter;
    private TestCaseExecutionContext? _testCaseExecutionContext;

    #region Tests
    public void Test(string testName, Action testFunction)
        => TestBase(testName, () => { testFunction(); return Task.CompletedTask; })
            .ConfigureAwait(false).GetAwaiter().GetResult();

    public async Task Test(string testName, Func<Task> testFunction)
        => await TestBase(testName, testFunction);

    private async Task TestBase(string testName, Func<Task> testFunction)
    {
        CheckIfTestCaseExecutionContextIsSet();

        var test = new Test(testName, testFunction);
        _testCaseExecutionContext.RegisterTest(test);

        await ExecuteTest(test);
    }

    private async Task ExecuteTest(Test test)
    {
        try
        {
            await ExecuteTest(test, test.Function);
        }
        catch (Exception ex)
        {
            TestFailure(test, ex);
        }
    }

    private void TestFailure(Test test, Exception ex)
    {
        test.Result = new TestResult.Failed(ex.Message, ex.StackTrace);
        _reporter.ReportTestFailure(test.Name, ex.Message);
    }

    private async Task ExecuteTest(Test test, Func<Task> testFunction)
    {
        _reporter.ReportTestStart(test.Name, _testCaseExecutionContext!.TestCase.RequestsFile.RelativePath);

        await testFunction();
        test.Result = new TestResult.Succeed();

        _reporter.ReportTestSuccess(test.Name);
    }
    #endregion

    public void SetCurrentTestCaseExecutionContext(TestCaseExecutionContext? testCaseExecutionContext)
        => _testCaseExecutionContext = testCaseExecutionContext;

    [MemberNotNull(nameof(_testCaseExecutionContext))]
    private void CheckIfTestCaseExecutionContextIsSet()
    => _ = _testCaseExecutionContext
        ?? throw new InvalidOperationException("Unable to execute test, if there is no test case assigned.");
}
