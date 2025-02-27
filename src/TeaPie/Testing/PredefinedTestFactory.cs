using TeaPie.Http;
using TeaPie.Http.Headers;
using static Xunit.Assert;

namespace TeaPie.Testing;

internal interface ITestFactory
{
    Test Create(TestDescription testDescription);

    void RegisterTestType(string testName, Func<HttpResponseMessage, object[], Task> testFunction);
}

internal class PredefinedTestFactory : ITestFactory
{
    private readonly IHeadersHandler _headersHandler;
    private readonly Dictionary<TestType, Func<TestDescription, Test>> _factoryMethods;
    private readonly Dictionary<string, Func<HttpResponseMessage, object[], Task>> _userDefinedFactoryMethods;
    private static int _factoryCount = 1;

    public PredefinedTestFactory(IHeadersHandler headersHandler)
    {
        _headersHandler = headersHandler;
        _factoryMethods = new()
        {
            { TestType.ExpectStatusCodes, CreateExpectStatusCodesTest },
            { TestType.HasBody, CreateHasBodyTest },
            { TestType.HasHeader, CreateHasHeaderTest }
        };

        _userDefinedFactoryMethods = [];
    }

    public Test Create(TestDescription testDescription)
    {
        if (testDescription.Type == TestType.Custom)
        {
            return _userDefinedFactoryMethods.TryGetValue(testDescription.Directive, out var factoryMethod)
                ? GenericFactoryMethod(testDescription, string.Empty, factoryMethod)
                : throw new InvalidOperationException(
                    $"Unable to create test for unsupported test type '{testDescription.Directive}'.");
        }
        else
        {
            return _factoryMethods.TryGetValue(testDescription.Type, out var factoryMethod)
                ? factoryMethod(testDescription)
                : throw new InvalidOperationException(
                    $"Unable to create test for unsupported test type '{testDescription.Type}'.");
        }
    }

    public void RegisterTestType(
        string testName,
        Func<HttpResponseMessage, object[], Task> testFunction)
        => _userDefinedFactoryMethods[testName] = testFunction;

    private Test CreateExpectStatusCodesTest(TestDescription description)
    {
        CheckParameters(description.Type, description, out var requestExecutionContext);
        var statusCodes = (int[])description.Parameters[0];

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
        string testName,
        Func<HttpResponseMessage, object[], Task> testFunction)
    {
        CheckParameters(description.Type, description, out var requestExecutionContext);
        var parameters = description.Parameters;

        var finalTestName = GetGenericTestName(
            requestExecutionContext.TestCaseExecutionContext?.TestCase.Name,
            requestExecutionContext.Name,
            testName);

        return CreateTest(finalTestName,
            async () => await testFunction(requestExecutionContext.Response!, description.Parameters));
    }

    private Test CreateHasBodyTest(TestDescription description)
    {
        CheckParameters(description.Type, description, out var requestExecutionContext);
        var isTrue = (bool)description.Parameters[0];

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
        CheckParameters(description.Type, description, out var requestExecutionContext);
        var headerName = (string)description.Parameters[0];

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
        TestType testType,
        TestDescription description,
        out RequestExecutionContext requestExecutionContext)
    {
        if (description.Parameters.Length == 0)
        {
            throw new InvalidOperationException(
                $"Unable to create '{testType.ToString().ParsePascalCase()}' test, if no parameter provided.");
        }

        if (description.RequestExecutionContext is null)
        {
            throw new InvalidOperationException(
                $"Unable to create '{testType.ToString().ParsePascalCase()}' test, if no request execution context provided.");
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

    private static string GetGenericTestName(
        string? testCaseName, string? requestName, string testName)
        => $"[{_factoryCount}] " + GetTestNamePrefix(testCaseName, requestName) +
            $"{testName}.";

    private static string GetTestNamePrefix(string? testCaseName, string? requestName)
        => string.IsNullOrEmpty(testCaseName)
            ? string.IsNullOrEmpty(requestName) ? string.Empty : $"{requestName}: "
            : string.IsNullOrEmpty(requestName) ? $"{testCaseName}: " : $"{testCaseName}-{requestName}: ";

    private static string GetBoolRepresentation(bool isTrue) => isTrue ? string.Empty : "not ";
}
