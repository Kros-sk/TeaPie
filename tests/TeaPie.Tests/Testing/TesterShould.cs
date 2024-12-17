using FluentAssertions;
using NSubstitute;
using TeaPie.Reporting;
using TeaPie.StructureExploration;
using TeaPie.TestCases;
using TeaPie.Testing;
using File = TeaPie.StructureExploration.File;

namespace TeaPie.Tests.Testing;

public class TesterShould
{
    private readonly string _mockPath;
    private readonly IReporter _mockReporter;
    private readonly TestCaseExecutionContext _mockTestCaseExecutionContext;
    private readonly ICurrentTestCaseExecutionContextAccessor _currentTestCaseExecutionContextAccessor;
    private readonly Tester _tester;

    public TesterShould()
    {
        _mockPath = "pathToTestCase.http";
        _mockReporter = Substitute.For<IReporter>();
        _mockTestCaseExecutionContext = new TestCaseExecutionContext(
            new TestCase(new File(_mockPath, _mockPath, _mockPath, null!)));

        _currentTestCaseExecutionContextAccessor = new CurrentTestCaseExecutionContextAccessor()
        {
            CurrentTestCaseExecutionContext = _mockTestCaseExecutionContext
        };

        _tester = new Tester(_mockReporter, _currentTestCaseExecutionContextAccessor);
    }

    [Fact]
    public void ReportTestStartAndTestSuccessWhenTestSucceed()
    {
        const string testName = "SyncTest";

        static void testFunction()
        {
            true.Should().BeTrue();
        }

        _tester.Test(testName, testFunction);

        _mockReporter.Received(1).ReportTestStart(testName, _mockPath);
        _mockReporter.Received(1).ReportTestSuccess(testName);
        _mockReporter.DidNotReceive().ReportTestFailure(testName, Arg.Any<string>());
    }

    [Fact]
    public async Task ReportTestStartAndTestSuccessWhenAsyncTestSucceed()
    {
        const string testName = "AsyncTest";

        static async Task testFunction()
        {
            await Task.Delay(100);
            true.Should().BeTrue();
        }

        await _tester.Test(testName, testFunction);

        _mockReporter.Received(1).ReportTestStart(testName, _mockPath);
        _mockReporter.Received(1).ReportTestSuccess(testName);
        _mockReporter.DidNotReceive().ReportTestFailure(testName, Arg.Any<string>());
    }

    [Fact]
    public void HandleExceptionAndReportFailureWhenTestFails()
    {
        const string testName = "SyncTestWithException";

        static void testFunction()
        {
            throw new InvalidOperationException("Test failed");
        }

        _tester.Test(testName, testFunction);

        _mockReporter.Received(1).ReportTestStart(testName, Arg.Any<string>());
        _mockReporter.Received(1).ReportTestFailure(testName, "Test failed");
        _mockReporter.DidNotReceive().ReportTestSuccess(testName);
    }

    [Fact]
    public async Task HandleExceptionAndReportFailureWhenAsyncTestFails()
    {
        const string testName = "AsyncTestWithException";

        static async Task testFunction()
        {
            await Task.Delay(100);
            throw new InvalidOperationException("Test failed");
        }

        await _tester.Test(testName, testFunction);

        _mockReporter.Received(1).ReportTestStart(testName, _mockPath);
        _mockReporter.Received(1).ReportTestFailure(testName, "Test failed");
        _mockReporter.DidNotReceive().ReportTestSuccess(testName);
    }

    [Fact]
    public void UseDifferentAssertionsButHaveSameBehavior()
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

        _mockReporter.Received(1).ReportTestStart(testName, _mockPath);
        _mockReporter.Received(1).ReportTestSuccess(testName);
        _mockReporter.DidNotReceive().ReportTestFailure(testName, Arg.Any<string>());
    }

    [Fact]
    public void ReportTestFailureWhenUsingDifferentAssertionsAndFails()
    {
        const string testName = "SyncTestWithMultipleAssertionsFailure";

        static void testFunction()
        {
            true.Should().BeFalse();
            2.Should().BeGreaterThan(3);
            "test".Should().BeEquivalentTo("fail");

            Assert.False(true);
            Assert.Equal(5, 3);
            Assert.Equal("test", "fail");
        }

        _tester.Test(testName, testFunction);

        _mockReporter.Received(1).ReportTestStart(testName, _mockPath);
        _mockReporter.Received(1).ReportTestFailure(testName, Arg.Any<string>());
        _mockReporter.DidNotReceive().ReportTestSuccess(testName);
    }
}
