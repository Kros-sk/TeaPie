using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using TeaPie.Reporting;
using TeaPie.StructureExploration;
using TeaPie.TestCases;
using TeaPie.Testing;
using static Xunit.Assert;

namespace TeaPie.Tests.Testing;

public class TesterShould
{
    private readonly string _mockPath;
    private readonly TestCaseExecutionContext _mockTestCaseExecutionContext;
    private readonly ICurrentTestCaseExecutionContextAccessor _currentTestCaseExecutionContextAccessor;
    private readonly ITestResultsSummaryReporter _mockReporter;
    private readonly ILogger<Tester> _mockLogger;
    private readonly Tester _tester;
    private readonly TestCase _testCase;

    public TesterShould()
    {
        _mockPath = "pathToTestCase.http";
        _testCase = new TestCase(new InternalFile(_mockPath, _mockPath, null!));
        _mockTestCaseExecutionContext = new TestCaseExecutionContext(_testCase);

        _currentTestCaseExecutionContextAccessor = new CurrentTestCaseExecutionContextAccessor()
        {
            Context = _mockTestCaseExecutionContext
        };

        _mockReporter = Substitute.For<ITestResultsSummaryReporter>();
        _mockLogger = Substitute.For<ILogger<Tester>>();
        _tester = new Tester(_currentTestCaseExecutionContextAccessor, _mockReporter, _mockLogger);
    }

    [Fact]
    public async Task ActuallyExecuteTestFunction()
    {
        var wasExecuted = false;

        void testFunction()
        {
            wasExecuted = true;
        }

        var test = CreateTest(
            string.Empty,
            false,
            () => { testFunction(); return Task.CompletedTask; });

        _mockTestCaseExecutionContext.RegisterTest(test);
        await _tester.ExecuteOrSkipTest(test, _testCase);

        True(wasExecuted);
    }

    [Fact]
    public async Task ActuallyExecuteAsyncTestFunction()
    {
        var wasExecuted = false;

        async Task testFunction()
        {
            wasExecuted = true;
            await Task.CompletedTask;
        }

        var test = CreateTest(string.Empty, false, testFunction);

        _mockTestCaseExecutionContext.RegisterTest(test);
        await _tester.ExecuteOrSkipTest(test, _testCase);

        True(wasExecuted);
    }

    [Fact]
    public async Task SkipTestIfRequested()
    {
        var wasExecuted = false;

        void testFunction()
        {
            wasExecuted = true;
        }

        var test = CreateTest(
            string.Empty,
            true,
            () => { testFunction(); return Task.CompletedTask; });

        _mockTestCaseExecutionContext.RegisterTest(test);
        await _tester.ExecuteOrSkipTest(test, _testCase);

        False(wasExecuted);
    }

    [Fact]
    public async Task SkipAsyncTestIfRequested()
    {
        var wasExecuted = false;

        async Task testFunction()
        {
            wasExecuted = true;
            await Task.CompletedTask;
        }

        var test = CreateTest(string.Empty, true, testFunction);

        _mockTestCaseExecutionContext.RegisterTest(test);
        await _tester.ExecuteOrSkipTest(test, _testCase);

        False(wasExecuted);
    }

    [Fact]
    public async Task CatchExceptionFromTest()
    {
        const string testName = "SyncTestWithException";

        static void testFunction()
        {
            throw new InvalidOperationException("Test failed");
        }

        var test = CreateTest(
            testName,
            false,
            () => { testFunction(); return Task.CompletedTask; });

        _mockTestCaseExecutionContext.RegisterTest(test);
        await _tester.ExecuteOrSkipTest(test, _testCase);
    }

    [Fact]
    public async Task CatchExceptionFromAsyncTest()
    {
        const string testName = "AsyncTestWithException";

        static async Task testFunction()
        {
            await Task.Delay(100);
            throw new InvalidOperationException("Test failed");
        }

        var test = CreateTest(testName, false, testFunction);

        _mockTestCaseExecutionContext.RegisterTest(test);
        await _tester.ExecuteOrSkipTest(test, _testCase);
    }

    [Fact]
    public async Task UseDifferentAssertionLanguageWithSameResults()
    {
        const string testName = "SyncTestWithMultipleAssertions";

        static void testFunction()
        {
            true.Should().BeTrue();
            5.Should().BeGreaterThan(3);
            "test".Should().BeEquivalentTo("test");

            True(true);
            NotEqual(5, 3);
            Equal("test", "test");
        }

        var test = CreateTest(
            testName,
            false,
            () => { testFunction(); return Task.CompletedTask; });

        _mockTestCaseExecutionContext.RegisterTest(test);
        await _tester.ExecuteOrSkipTest(test, _testCase);
    }

    [Fact]
    public void ExecuteTestSyncReturnsTrueWhenPasses()
    {
        var testName = "SyncPassingTest";
        var test = CreateTest(testName, false, () => Task.CompletedTask);

        _mockTestCaseExecutionContext.RegisterTest(test);
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        var result = _tester.ExecuteTestSync(testName, response);

        True(result);
    }

    [Fact]
    public void ExecuteTestSyncReturnsFalseWhenFails()
    {
        var testName = "SyncFailingTest";
        var test = CreateTest(
            testName,
            false,
            () => throw new InvalidOperationException("Sync test failed"));

        _mockTestCaseExecutionContext.RegisterTest(test);
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        var result = _tester.ExecuteTestSync(testName, response);

        False(result);
    }

    [Fact]
    public void ExecuteTestSyncThrowsWhenTestNotFound()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        var exception = Throws<InvalidOperationException>(() =>
            _tester.ExecuteTestSync("NonExistentTest", response));

        Equal("Test \"NonExistentTest\" doesn't exists.", exception.Message);
    }

    private Test CreateTest(
        string name,
        bool skipTest,
        Func<Task> function,
        TestResult? result = null,
        TestCase? testCase = null)
    {
        result ??= new TestResult.NotRun { TestName = name, TestCasePath = _mockPath };
        testCase ??= _testCase;
        return new Test(name, skipTest, function, result, testCase);
    }
}
