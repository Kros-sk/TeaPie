using System.Text.RegularExpressions;
using TeaPie.Http.Headers;
using TeaPie.Testing;
using static Xunit.Assert;
using Consts = TeaPie.Http.Parsing.HttpFileParserConstants;

namespace TeaPie.Http.Parsing;

internal static partial class DefaultDirectivesProvider
{
    public static List<TestDirective> GetDefaultTestDirectives()
        =>
        [
            new(
                Consts.TestExpectStatusCodesDirectiveFullName,
                Consts.TestExpectStatusCodesDirectivePattern,
                GetExpectStatusCodesTestName,
                GetExpectStatusCodesTestFunction),
            new (
                Consts.TestHasBodyDirectiveFullName,
                Consts.TestHasBodyDirectivePattern,
                GetHasBodyTestName,
                GetHasBodyTestFunction),
            new (
                Consts.TestHasBodyNoParameterInternalDirectiveFullName,
                Consts.TestHasBodyNoParameterDirectivePattern,
                GetHasBodyTestName,
                GetHasBodyTestFunction),
            new (
                Consts.TestHasHeaderDirectiveFullName,
                Consts.TestHasHeaderDirectivePattern,
                GetHasHeaderTestName,
                GetHasHeaderTestFunction),
        ];

    private static async Task GetExpectStatusCodesTestFunction(
        HttpResponseMessage response, IReadOnlyDictionary<string, string> parameters)
    {
        var statusCodesText = parameters[Consts.TestExpectStatusCodesParameterName];

        var statusCodes = NumberPattern().Matches(statusCodesText)
            .Select(m => int.Parse(m.Value))
            .ToArray();

        True(statusCodes.Contains(response.StatusCode()));
        await Task.CompletedTask;
    }

    private static async Task GetHasBodyTestFunction(HttpResponseMessage response, IReadOnlyDictionary<string, string> parameters)
    {
        var isTrue = true;
        if (parameters.TryGetValue(Consts.TestHasBodyDirectiveParameterName, out var parameter))
        {
            isTrue = bool.Parse(parameter);
        }

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
        if (!parameters.TryGetValue(Consts.TestHasHeaderDirectiveParameterName, out var parameter) ||
            parameter is not string headerName)
        {
            throw new InvalidOperationException(
                $"Unable to retrieve parameter '{Consts.TestHasHeaderDirectiveParameterName.SplitPascalCase()}'");
        }

        False(string.IsNullOrEmpty(HeadersHandler.GetHeaderFromResponse(headerName, response)));
        await Task.CompletedTask;
    }

    private static string GetExpectStatusCodesTestName(IReadOnlyDictionary<string, string> parameters)
        => $"Status code should match one of these: {parameters[Consts.TestExpectStatusCodesParameterName]}";

    private static string GetHasBodyTestName(IReadOnlyDictionary<string, string> parameters)
    {
        var isTrue = true;
        if (parameters.TryGetValue(Consts.TestHasBodyDirectiveParameterName, out var parameter))
        {
            isTrue = bool.Parse(parameter);
        }

        return $"Response should {GetBoolRepresentation(isTrue)}have body.";
    }

    private static string GetHasHeaderTestName(IReadOnlyDictionary<string, string> parameters)
        => $"Response should have header with name '{parameters[Consts.TestHasHeaderDirectiveParameterName]}'.";

    private static string GetBoolRepresentation(bool isTrue) => isTrue ? string.Empty : "not ";

    [GeneratedRegex(@"\d+")]
    private static partial Regex NumberPattern();
}
