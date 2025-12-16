using NSubstitute;
using TeaPie.Scripts;
using TeaPie.StructureExploration;
using TeaPie.Reporting;
using TeaPie.TestCases;
using TeaPie.Testing;

namespace TeaPie.Tests.Scripts;

public class RunScriptTestsStepShould
{
    private readonly ITestCaseExecutionContextAccessor _accessor;
    private readonly ITestResultsSummaryReporter _mockReporter;
    private readonly ITester _mockTester;
    private readonly RunScriptTestsStep _step;
    private readonly TestCaseExecutionContext _context;
    private readonly TestCase _testCase;
    private readonly ApplicationContext _appContext;

    public RunScriptTestsStepShould()
    {
        _testCase = new TestCase(new InternalFile("test.http", "test.http", null!));
        _context = new TestCaseExecutionContext(_testCase);

        _accessor = Substitute.For<ITestCaseExecutionContextAccessor>();
        _accessor.Context.Returns(_context);

        _mockReporter = Substitute.For<ITestResultsSummaryReporter>();
        _mockTester = Substitute.For<ITester>();

        _step = new RunScriptTestsStep(_accessor, _mockReporter, _mockTester);
        _appContext = new ApplicationContextBuilder().Build();
    }

    [Fact]
    public async Task InitializeReporterBeforeExecutingTests()
    {
        await _step.Execute(_appContext);

        Received.InOrder(() =>
        {
            _mockReporter.Initialize();
        });
    }

    [Fact]
    public async Task ExecuteAllRegisteredTests()
    {
        var test1 = CreateTest("test1");
        var test2 = CreateTest("test2");
        var test3 = CreateTest("test3");

        _context.RegisterTest(test1);
        _context.RegisterTest(test2);
        _context.RegisterTest(test3);

        await _step.Execute(_appContext);

        await _mockTester.Received(1).ExecuteOrSkipTest(test1, _testCase);
        await _mockTester.Received(1).ExecuteOrSkipTest(test2, _testCase);
        await _mockTester.Received(1).ExecuteOrSkipTest(test3, _testCase);
    }

    [Fact]
    public async Task ExecuteTestsInRegistrationOrder()
    {
        var test1 = CreateTest("test1");
        var test2 = CreateTest("test2");
        var test3 = CreateTest("test3");

        _context.RegisterTest(test1);
        _context.RegisterTest(test2);
        _context.RegisterTest(test3);

        await _step.Execute(_appContext);

        Received.InOrder(async () =>
        {
            await _mockTester.ExecuteOrSkipTest(test1, _testCase);
            await _mockTester.ExecuteOrSkipTest(test2, _testCase);
            await _mockTester.ExecuteOrSkipTest(test3, _testCase);
        });
    }

    [Fact]
    public async Task HandleEmptyTestCollection()
    {
        // No tests registered
        await _step.Execute(_appContext);

        await _mockTester.DidNotReceive().ExecuteOrSkipTest(Arg.Any<Test>(), Arg.Any<TestCase>());
        _mockReporter.Received(1).Initialize();
    }

    [Fact]
    public async Task PassCorrectTestCaseToTester()
    {
        var test = CreateTest("test");
        _context.RegisterTest(test);

        await _step.Execute(_appContext);

        await _mockTester.Received(1).ExecuteOrSkipTest(Arg.Any<Test>(), _testCase);
    }

    [Fact]
    public async Task ExecuteSkippedTests()
    {
        var skippedTest = CreateTest("skipped", skipTest: true);
        _context.RegisterTest(skippedTest);

        await _step.Execute(_appContext);

        await _mockTester.Received(1).ExecuteOrSkipTest(skippedTest, _testCase);
    }

    [Fact]
    public async Task ExecuteBothSkippedAndNonSkippedTests()
    {
        var normalTest = CreateTest("normal", skipTest: false);
        var skippedTest = CreateTest("skipped", skipTest: true);

        _context.RegisterTest(normalTest);
        _context.RegisterTest(skippedTest);

        await _step.Execute(_appContext);

        await _mockTester.Received(1).ExecuteOrSkipTest(normalTest, _testCase);
        await _mockTester.Received(1).ExecuteOrSkipTest(skippedTest, _testCase);
    }

    private Test CreateTest(string name, bool skipTest = false)
    {
        var result = new TestResult.NotRun { TestName = name, TestCasePath = "test.http" };
        return new Test(name, skipTest, () => Task.CompletedTask, result, _testCase);
    }
}
