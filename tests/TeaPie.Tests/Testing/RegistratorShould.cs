using TeaPie.StructureExploration;
using TeaPie.TestCases;
using TeaPie.Testing;
using static Xunit.Assert;

namespace TeaPie.Tests.Testing;

public class RegistratorShould
{
    private readonly string _mockPath;
    private readonly TestCaseExecutionContext _mockTestCaseExecutionContext;
    private readonly ICurrentTestCaseExecutionContextAccessor _currentTestCaseExecutionContextAccessor;
    private readonly Registrator _registrator;

    public RegistratorShould()
    {
        _mockPath = "pathToTestCase.http";
        _mockTestCaseExecutionContext = new TestCaseExecutionContext(
            new TestCase(new InternalFile(_mockPath, _mockPath, null!)));

        _currentTestCaseExecutionContextAccessor = new CurrentTestCaseExecutionContextAccessor()
        {
            Context = _mockTestCaseExecutionContext
        };

        _registrator = new Registrator(_currentTestCaseExecutionContextAccessor);
    }

    [Fact]
    public void RegisterTestFunction()
    {
        var testName = "testRegistration";
        _registrator.Test(testName, () => { });

        var test = _mockTestCaseExecutionContext.GetTest(testName);
        NotNull(test);
        Equal(testName, test.Name);
    }

    [Fact]
    public void RegisterTestWithSkipFlag()
    {
        var testName = "skippedTest";
        _registrator.Test(testName, () => { }, skipTest: true);

        var test = _mockTestCaseExecutionContext.GetTest(testName);
        NotNull(test);
        True(test.SkipTest);
    }

    [Fact]
    public void RegisterTestWithNotRunResult()
    {
        var testName = "testWithNotRunResult";
        _registrator.Test(testName, () => { });

        var test = _mockTestCaseExecutionContext.GetTest(testName);
        NotNull(test);
        IsType<TestResult.NotRun>(test.Result);
    }

    [Fact]
    public void RegisterTestWithCorrectTestCasePath()
    {
        var testName = "testWithPath";
        _registrator.Test(testName, () => { });

        var test = _mockTestCaseExecutionContext.GetTest(testName);
        NotNull(test);
        Equal(_mockPath, test.Result.TestCasePath);
    }

    [Fact]
    public void ThrowExceptionWhenContextIsNull()
    {
        var accessorWithNullContext = new CurrentTestCaseExecutionContextAccessor();
        var registrator = new Registrator(accessorWithNullContext);

        var exception = Throws<InvalidOperationException>(() => registrator.Test("test", () => { }));
        Equal("Unable to test if no test-case execution context is provided.", exception.Message);
    }

    [Fact]
    public void ThrowExceptionOnDuplicateRegistration()
    {
        var testName = "duplicateTest";
        _registrator.Test(testName, () => { });

        Throws<ArgumentException>(() => _registrator.Test(testName, () => { }));
    }

    [Fact]
    public void RegisterMultipleTestsWithUniqueNames()
    {
        _registrator.Test("test1", () => { });
        _registrator.Test("test2", () => { });
        _registrator.Test("test3", () => { });

        NotNull(_mockTestCaseExecutionContext.GetTest("test1"));
        NotNull(_mockTestCaseExecutionContext.GetTest("test2"));
        NotNull(_mockTestCaseExecutionContext.GetTest("test3"));
    }

    [Fact]
    public void SetTestCaseReferenceOnTest()
    {
        var testName = "testWithTestCase";
        _registrator.Test(testName, () => { });

        var test = _mockTestCaseExecutionContext.GetTest(testName);
        NotNull(test);
        NotNull(test.TestCase);
        Equal(_mockPath, test.TestCase.RequestsFile.RelativePath);
    }
}
