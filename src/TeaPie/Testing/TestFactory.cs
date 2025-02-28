using TeaPie.Http;
using TeaPie.Http.Headers;
using TeaPie.Http.Parsing;
using static Xunit.Assert;

namespace TeaPie.Testing;

internal interface ITestFactory
{
    Test Create(TestDescription testDescription);

    void RegisterTestType(string testName, Func<HttpResponseMessage, IReadOnlyDictionary<string, object>, Task> testFunction);
}

internal class TestFactory : ITestFactory
{
    private readonly IHeadersHandler _headersHandler;
    private readonly Dictionary<string, Func<TestDescription, Test>> _factoryMethods;
    private readonly
        Dictionary<string, Func<HttpResponseMessage, IReadOnlyDictionary<string, object>, Task>> _userDefinedFactoryMethods;

    private static int _factoryCount = 1;

    public TestFactory(IHeadersHandler headersHandler)
    {
        _headersHandler = headersHandler;
        _factoryMethods = [];
        _userDefinedFactoryMethods = [];
    }

    public Test Create(TestDescription testDescription)
    {
        return _userDefinedFactoryMethods.TryGetValue(testDescription.Directive, out var factoryMethod)
            ? GenericFactoryMethod(testDescription, string.Empty, factoryMethod)
            : throw new InvalidOperationException(
                $"Unable to create test for unsupported test type '{testDescription.Directive}'.");

        //return _factoryMethods.TryGetValue(testDescription.Type, out var factoryMethod)
        //    ? factoryMethod(testDescription)
        //    : throw new InvalidOperationException(
        //        $"Unable to create test for unsupported test type '{testDescription.Type}'.");
    }

    public void RegisterTestType(
        string testName,
        Func<HttpResponseMessage, IReadOnlyDictionary<string, object>, Task> testFunction)
        => _userDefinedFactoryMethods[testName] = testFunction;

    private Test CreateExpectStatusCodesTest(TestDescription description)
    {
        CheckParameters(description, out var requestExecutionContext);
        var statusCodes = (int[])description.Parameters[HttpFileParserConstants.TestExpectStatusCodesSectionName];

        async Task testFunction()
        {
            True(statusCodes.Contains(requestExecutionContext.Response!.StatusCode()));
            await Task.CompletedTask;
        }

        var testName = GetExpectStatusCodesTestName(
            requestExecutionContext.TestCaseExecutionContext?.TestCase.Name,
            requestExecutionContext.Name,
            statusCodes);

        return CreateTest(testName, testFunction);
    }

    private static Test GenericFactoryMethod(
        TestDescription description,
        Func<IReadOnlyDictionary<string, object>, string> testNameGetter,
        Func<HttpResponseMessage, IReadOnlyDictionary<string, object>, Task> testFunction)
    {
        CheckParameters(description, out var requestExecutionContext);

        return CreateTest(testNameGetter(description.Parameters),
            async () => await testFunction(requestExecutionContext.Response!, description.Parameters));
    }

    private Test CreateHasBodyTest(TestDescription description)
    {
        CheckParameters(description, out var requestExecutionContext);

        var isTrue = true;
        if (description.Parameters.ToDictionary().ContainsKey(HttpFileParserConstants.TestHasBodyDirectiveSectionName))
        {
            isTrue = (bool)description.Parameters[HttpFileParserConstants.TestHasBodyDirectiveSectionName];
        }

        async Task testFunction()
        {
            if (isTrue)
            {
                NotNull(requestExecutionContext.Response!.Content);
            }
            else
            {
                Null(requestExecutionContext.Response!.Content);
            }
            await Task.CompletedTask;
        }

        var testName = GetHasBodyTestName(
            requestExecutionContext.TestCaseExecutionContext?.TestCase.Name,
            requestExecutionContext.Name,
            isTrue);

        return CreateTest(testName, testFunction);
    }

    private Test CreateHasHeaderTest(TestDescription description)
    {
        CheckParameters(description, out var requestExecutionContext);
        var headerName = (string)description.Parameters[HttpFileParserConstants.TestHasHeaderDirectiveSectionName];

        async Task testFunction()
        {
            False(string.IsNullOrEmpty(_headersHandler.GetHeader(headerName, requestExecutionContext.Response!)));
            await Task.CompletedTask;
        }

        var testName = GetHasHeaderTestName(
            requestExecutionContext.TestCaseExecutionContext?.TestCase.Name,
            requestExecutionContext.Name,
            headerName);

        return CreateTest(testName, testFunction);
    }

    private static Test CreateTest(string testName, Func<Task> testFunction)
    {
        var test = new Test(testName, testFunction, new TestResult.NotRun() { TestName = testName });
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
                $"Unable to create test, if no request execution context provided.");
        }

        requestExecutionContext = description.RequestExecutionContext;
    }

    private static string GetExpectStatusCodesTestName(string? testCaseName, string? requestName, int[] statusCodes)
        => $"[{_factoryCount}] " + GetTestNamePrefix(testCaseName, requestName) +
            $"Status code should be one of these: [{string.Join(", ", statusCodes)}]";

    private static string GetHasBodyTestName(string? testCaseName, string? requestName, bool isTrue)
        => $"[{_factoryCount}] " + GetTestNamePrefix(testCaseName, requestName) +
            $"Response should {GetBoolRepresentation(isTrue)}have body.";

    private static string GetHasHeaderTestName(string? testCaseName, string? requestName, string headerName)
        => $"[{_factoryCount}] " + GetTestNamePrefix(testCaseName, requestName) +
            $"Response should have header with name '{headerName}'.";

    private static string GetTestNamePrefix(string? testCaseName, string? requestName)
        => string.IsNullOrEmpty(testCaseName)
            ? string.IsNullOrEmpty(requestName) ? string.Empty : $"{requestName}: "
            : string.IsNullOrEmpty(requestName) ? $"{testCaseName}: " : $"{testCaseName}-{requestName}: ";

    private static string GetBoolRepresentation(bool isTrue) => isTrue ? string.Empty : "not ";
}
