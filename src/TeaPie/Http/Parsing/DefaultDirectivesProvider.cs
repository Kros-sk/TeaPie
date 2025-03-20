using System.Data;
using System.Text.RegularExpressions;
using TeaPie.Http.Headers;
using TeaPie.Testing;
using static Xunit.Assert;

namespace TeaPie.Http.Parsing;

internal static partial class DefaultDirectivesProvider
{
    public static List<TestDirective> GetDefaultTestDirectives()
        =>
        [
            new(
                TestDirectives.TestExpectStatusCodesDirectiveFullName,
                TestDirectives.TestExpectStatusCodesDirectivePattern,
                GetExpectStatusCodesTestName,
                GetExpectStatusCodesTestFunction),
            new (
                TestDirectives.TestHasBodyDirectiveFullName,
                TestDirectives.TestHasBodyDirectivePattern,
                GetHasBodyTestName,
                GetHasBodyTestFunction),
            new (
                TestDirectives.TestHasBodyNoParameterInternalDirectiveFullName,
                TestDirectives.TestHasBodyNoParameterDirectivePattern,
                GetHasBodyTestName,
                GetHasBodyTestFunction),
            new (
                TestDirectives.TestHasHeaderDirectiveFullName,
                TestDirectives.TestHasHeaderDirectivePattern,
                GetHasHeaderTestName,
                GetHasHeaderTestFunction),
        ];

    private static async Task GetExpectStatusCodesTestFunction(
        HttpResponseMessage response, IReadOnlyDictionary<string, string> parameters)
    {
        var statusCodesText = parameters[TestDirectives.TestExpectStatusCodesParameterName];

        var statusCodes = NumberPattern().Matches(statusCodesText)
            .Select(m => int.Parse(m.Value))
            .ToArray();

        if (statusCodes.Length == 0)
        {
            throw new SyntaxErrorException("Test directive uses requires at least one status code provided in the array.");
        }

        var foundStatusCode = statusCodes[0];
        foreach (var statusCode in statusCodes)
        {
            foundStatusCode = statusCode;
            if (statusCode == response.StatusCode())
            {
                break;
            }
        }

        Equal(foundStatusCode, response.StatusCode());
        await Task.CompletedTask;
    }

    private static async Task GetHasBodyTestFunction(HttpResponseMessage response, IReadOnlyDictionary<string, string> parameters)
    {
        var isTrue = GetBool(parameters);

        if (isTrue)
        {
            NotNull(response.Content);
        }
        else
        {
            Null(response.Content);
        }

        await Task.CompletedTask;
    }

    private static async Task GetHasHeaderTestFunction(
        HttpResponseMessage response, IReadOnlyDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue(TestDirectives.TestHasHeaderDirectiveParameterName, out var parameter) ||
            parameter is not string headerName)
        {
            throw new InvalidOperationException(
                $"Unable to retrieve parameter '{TestDirectives.TestHasHeaderDirectiveParameterName.SplitPascalCase()}'");
        }

        False(string.IsNullOrEmpty(HeadersHandler.GetHeaderFromResponse(headerName, response)));
        await Task.CompletedTask;
    }

    private static string GetExpectStatusCodesTestName(IReadOnlyDictionary<string, string> parameters)
        => $"Status code should match one of these: {parameters[TestDirectives.TestExpectStatusCodesParameterName]}";

    private static string GetHasBodyTestName(IReadOnlyDictionary<string, string> parameters)
        => $"Response should {GetNegationIfNeeded(GetBool(parameters))}have body.";

    private static string GetHasHeaderTestName(IReadOnlyDictionary<string, string> parameters)
        => $"Response should have header with name '{parameters[TestDirectives.TestHasHeaderDirectiveParameterName]}'.";

    private static string GetNegationIfNeeded(bool isTrue) => isTrue ? string.Empty : "not ";

    private static bool GetBool(IReadOnlyDictionary<string, string> parameters)
    {
        var isTrue = true;
        if (parameters.TryGetValue(TestDirectives.TestHasBodyDirectiveParameterName, out var parameter))
        {
            isTrue = bool.Parse(parameter);
        }

        return isTrue;
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex NumberPattern();
}
