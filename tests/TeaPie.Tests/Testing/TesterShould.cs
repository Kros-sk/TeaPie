using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Reporting;
using TeaPie.StructureExploration;
using TeaPie.TestCases;
using TeaPie.Testing;
using static Xunit.Assert;
using File = TeaPie.StructureExploration.File;

namespace TeaPie.Tests.Testing;

public class TesterShould
{
    private readonly string _mockPath;
    private readonly TestCaseExecutionContext _mockTestCaseExecutionContext;
    private readonly ICurrentTestCaseExecutionContextAccessor _currentTestCaseExecutionContextAccessor;
    private readonly Tester _tester;

    public TesterShould()
    {
        _mockPath = "pathToTestCase.http";
        _mockTestCaseExecutionContext = new TestCaseExecutionContext(
            new TestCase(new File(_mockPath, _mockPath, _mockPath, null!)));

        _currentTestCaseExecutionContextAccessor = new CurrentTestCaseExecutionContextAccessor()
        {
            Context = _mockTestCaseExecutionContext
        };

        _tester = new Tester(
            _currentTestCaseExecutionContextAccessor,
            Substitute.For<ITestResultsSummaryReporter>(),
            Substitute.For<ILogger<Tester>>());
    }

    [Fact]
    public void ActuallyExecuteTestFunction()
    {
        var wasExecuted = false;

        void testFunction()
        {
            wasExecuted = true;
        }

        _tester.Test(string.Empty, testFunction);
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

        await _tester.Test(string.Empty, testFunction);
        True(wasExecuted);
    }

    [Fact]
    public void SkipTestIfRequested()
    {
        var wasExecuted = false;

        void testFunction()
        {
            wasExecuted = true;
        }

        _tester.Test(string.Empty, testFunction, true);
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

        await _tester.Test(string.Empty, testFunction, true);
        False(wasExecuted);
    }

    [Fact]
    public void RegisterTestToCurrentExecutionContext()
    {
        const string testName = "SyncTest";

        static void testFunction()
        {
            True(true);
        }

        var test = new Test(testName, () => Task.FromResult(testFunction), new TestResult.Passed(10) { TestName = testName });

        _tester.Test(testName, testFunction);

        // If test was already registered, attempt to register it again should fail.
        Throws<ArgumentException>(() => _mockTestCaseExecutionContext.RegisterTest(test));
    }

    [Fact]
    public async Task RegisterAsyncTestToCurrentExecutionContext()
    {
        const string testName = "AsyncTest";

        static async Task testFunction()
        {
            True(true);
            await Task.CompletedTask;
        }

        var test = new Test(testName, testFunction, new TestResult.Passed(10) { TestName = testName });

        await _tester.Test(testName, testFunction);

        // If test was already registered, attempt to register it again should fail.
        Throws<ArgumentException>(() => _mockTestCaseExecutionContext.RegisterTest(test));
    }

    [Fact]
    public void CatchExceptionFromTest()
    {
        const string testName = "SyncTestWithException";

        static void testFunction()
        {
            throw new InvalidOperationException("Test failed");
        }

        _tester.Test(testName, testFunction);
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

        await _tester.Test(testName, testFunction);
    }

    [Fact]
    public void UseDifferentAssertionLanguageWithSameResults()
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

        _tester.Test(testName, testFunction);
    }
}
