using TeaPie.Http;
using TeaPie.Http.Headers;
using static Xunit.Assert;

namespace TeaPie.Testing;

internal interface IPredefinedTestFactory
{
    Test Create(PredefinedTestDescription testDescription);
}

internal class PredefinedTestFactory : IPredefinedTestFactory
{
    private readonly IHeadersHandler _headersHandler;
    private readonly Dictionary<PredefinedTestType, Func<PredefinedTestDescription, Test>> _factoryMethods;
    private static int _factoryCount = 1;

    public PredefinedTestFactory(IHeadersHandler headersHandler)
    {
        _headersHandler = headersHandler;
        _factoryMethods = new()
        {
            { PredefinedTestType.ExpectStatusCodes, CreateExpectStatusCodesTest },
            { PredefinedTestType.HasBody, CreateHasBodyTest },
            { PredefinedTestType.HasHeader, CreateHasHeaderTest }
        };
    }

    public Test Create(PredefinedTestDescription testDescription)
        => _factoryMethods.TryGetValue(testDescription.Type, out var factoryMethod)
            ? factoryMethod(testDescription)
            : throw new InvalidOperationException($"Unable to create test for unsupported test type '{testDescription.Type}'.");

    private Test CreateExpectStatusCodesTest(PredefinedTestDescription description)
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

    private Test CreateHasBodyTest(PredefinedTestDescription description)
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

    private Test CreateHasHeaderTest(PredefinedTestDescription description)
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
        PredefinedTestType testType,
        PredefinedTestDescription description,
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

    private static string GetTestNamePrefix(string? testCaseName, string? requestName)
        => string.IsNullOrEmpty(testCaseName)
            ? string.IsNullOrEmpty(requestName) ? string.Empty : $"{requestName}: "
            : string.IsNullOrEmpty(requestName) ? $"{testCaseName}: " : $"{testCaseName}-{requestName}: ";

    private static string GetBoolRepresentation(bool isTrue) => isTrue ? string.Empty : "not ";
}
