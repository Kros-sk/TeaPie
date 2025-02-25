using TeaPie.Http;
using static Xunit.Assert;

namespace TeaPie.Testing;

internal static class PredefinedTestFactory
{
    private static readonly Dictionary<PredefinedTestType, Func<PredefinedTestDescription, Test>> _factoryMethods = new()
    {
        { PredefinedTestType.ExpectStatusCodes, CreateExpectStatusCodesTest }
    };

    public static Test Create(PredefinedTestDescription testDescription)
        => _factoryMethods.TryGetValue(testDescription.Type, out var factoryMethod)
            ? factoryMethod(testDescription)
            : throw new InvalidOperationException($"Unable to create test for unsupported test type '{testDescription.Type}'.");

    private static Test CreateExpectStatusCodesTest(PredefinedTestDescription description)
    {
        CheckParameters(description, out var requestExecutionContext);
        var statusCodes = ResolveStatusCodes(description);

        async Task testFunction()
        {
            True(statusCodes.Contains(requestExecutionContext.Response!.StatusCode()));
            await Task.CompletedTask;
        }

        var testName = GetExpectStatusCodesTestName(
            requestExecutionContext.TestCaseExecutionContext?.TestCase.Name,
            requestExecutionContext.Name,
            statusCodes);

        return new Test(testName, testFunction, new TestResult.NotRun() { TestName = testName });
    }

    private static void CheckParameters(
        PredefinedTestDescription description, out RequestExecutionContext requestExecutionContext)
    {
        if (description.Parameters.Length == 0)
        {
            throw new InvalidOperationException("Unable to create 'Expect Status Codes' test, if no status codes provided.");
        }

        if (description.RequestExecutionContext is null)
        {
            throw new InvalidOperationException(
                "Unable to create 'Expect Status Codes' test, if no request execution context provided.");
        }

        requestExecutionContext = description.RequestExecutionContext;
    }

    private static int[] ResolveStatusCodes(PredefinedTestDescription description)
        => (int[])description.Parameters[0];

    private static string GetExpectStatusCodesTestName(string? testCaseName, string? requestName, int[] statusCodes)
        => GetTestCaseNamePrefix(testCaseName) +
            GetRequestNamePrefix(requestName) +
            $"Status code should be one of these: [{string.Join(", ", statusCodes)}]";

    private static string GetTestCaseNamePrefix(string? name) => string.IsNullOrEmpty(name) ? string.Empty : name + ": ";

    private static string GetRequestNamePrefix(string? name) => string.IsNullOrEmpty(name) ? string.Empty : "-" + name;
}
