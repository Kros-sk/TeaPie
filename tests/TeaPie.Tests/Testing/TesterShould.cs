using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TeaPie.Reporting;
using TeaPie.StructureExploration;
using TeaPie.TestCases;
using TeaPie.Testing;
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
            Substitute.For<ITestsResultsSummaryReporter>(),
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
        wasExecuted.Should().BeTrue();
    }

    [Fact]
    public void RegisterTestToCurrentExecutionContext()
    {
        const string testName = "SyncTest";

        static void testFunction()
        {
            true.Should().BeTrue();
        }

        var test = new Test(testName, () => Task.FromResult(testFunction), new TestResult.Passed(testName, 10));

        _tester.Test(testName, testFunction);

        // If test was already registered, attempt to register it again should fail.
        _mockTestCaseExecutionContext.Invoking(c => c.RegisterTest(test)).Throws<ArgumentException>();
    }

    [Fact]
    public async Task RegisterAsyncTestToCurrentExecutionContext()
    {
        const string testName = "AsyncTest";

        static async Task testFunction()
        {
            true.Should().BeTrue();
            await Task.CompletedTask;
        }

        var test = new Test(testName, testFunction, new TestResult.Passed(testName, 10));

        await _tester.Test(testName, testFunction);

        // If test was already registered, attempt to register it again should fail.
        _mockTestCaseExecutionContext.Invoking(c => c.RegisterTest(test)).Throws<ArgumentException>();
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

            Assert.True(true);
            Assert.NotEqual(5, 3);
            Assert.Equal("test", "test");
        }

        _tester.Test(testName, testFunction);
    }
}
