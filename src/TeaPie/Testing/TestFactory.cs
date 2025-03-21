using TeaPie.Http;
using TeaPie.Http.Parsing;
using TeaPie.StructureExploration;

namespace TeaPie.Testing;

internal interface ITestFactory
{
    Test Create(TestDescription testDescription);

    void RegisterTestDirective(TestDirective testDirective);
}

internal class TestFactory : ITestFactory
{
    private readonly Dictionary<string, TestDirective> _supportedDirectives =
        DefaultDirectivesProvider.GetDefaultTestDirectives().ToDictionary(x => x.Name, y => y);

    private static int _factoryCount = 1;

    public Test Create(TestDescription testDescription)
        => _supportedDirectives.TryGetValue(testDescription.Directive, out var testDirective)
            ? Create(testDirective, testDescription)
            : throw new InvalidOperationException(
                $"Unable to create test for unsupported test directive '{testDescription.Directive}'.");

    public void RegisterTestDirective(TestDirective testDirective)
        => _supportedDirectives[testDirective.Name] = testDirective;

    private static Test Create(
        TestDirective testDirective,
        TestDescription description)
    {
        CheckParameters(description, out var requestExecutionContext);

        return CreateTest(testDirective.TestNameGetter(description.Parameters),
            requestExecutionContext.TestCaseExecutionContext?.TestCase,
            async () => await testDirective.TestFunction(requestExecutionContext.Response!, description.Parameters));
    }

    private static Test CreateTest(string testName, TestCase? testCase, Func<Task> testFunction)
    {
        var test = new Test(
            $"[{_factoryCount}] {testName}",
            testFunction,
            new TestResult.NotRun()
            {
                TestName = testName,
                TestCasePath = testCase?.RequestsFile.RelativePath ?? string.Empty
            },
            testCase);

        _factoryCount++;
        return test;
    }

    private static void CheckParameters(
        TestDescription description,
        out RequestExecutionContext requestExecutionContext)
    {
        if (description.RequestExecutionContext is null)
        {
            throw new InvalidOperationException(
                "Unable to create test, if no request execution context provided.");
        }

        requestExecutionContext = description.RequestExecutionContext;
    }
}
