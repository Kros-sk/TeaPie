using TeaPie.Testing;
using static Xunit.Assert;
using Consts = TeaPie.Http.Parsing.HttpFileParserConstants;

namespace TeaPie.Http.Parsing;

internal static class DefaultDirectivesProvider
{
    public static List<TestDirective> GetDefaultTestDirectives()
        =>
        [
            new(
                Consts.TestExpectStatusCodesDirectiveName,
                Consts.TestExpectStatusCodesDirectivePattern,
                GetExpectStatusCodesTestName,
                GetExpectStatusCodesTestFunction
            ),
            new (
                Consts.TestHasBodyDirectiveName,
                Consts.TestHasBodyDirectivePattern,
                GetHasBodyTestName,
                GetHasBodyTestFunction),
            new (
                Consts.TestHasHeaderDirectiveName,
                Consts.TestHasHeaderDirectivePattern,
                GetHasHeaderTestName,
                GetHasHeaderTestFunction),
        ];

    private static async Task GetExpectStatusCodesTestFunction(
        HttpResponseMessage response, IReadOnlyDictionary<string, object> parameters)
    {
        var statusCodes = (int[])parameters[Consts.TestExpectStatusCodesSectionName];
        True(statusCodes.Contains(response.StatusCode()));
        await Task.CompletedTask;
    }

    private static async Task GetHasBodyTestFunction(HttpResponseMessage response, IReadOnlyDictionary<string, object> parameters)
    {
        if (!parameters.TryGetValue(Consts.TestHasBodyDirectiveSectionName, out var isTrue))
        {
            isTrue = false;
        }

        if ((bool)isTrue)
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
        HttpResponseMessage message, IReadOnlyDictionary<string, object> dictionary)
    {
        // False(string.IsNullOrEmpty(_headersHandler.GetHeader(headerName, requestExecutionContext.Response!)));
        await Task.CompletedTask;
    }

    private static string GetExpectStatusCodesTestName(IReadOnlyDictionary<string, object> parameters)
        => "Status code should be one of these: [" +
            string.Join(", ", (int[])parameters[Consts.TestExpectStatusCodesSectionName]) + "]";

    private static string GetHasBodyTestName(IReadOnlyDictionary<string, object> parameters)
    {
        if (!parameters.TryGetValue(Consts.TestHasBodyDirectiveSectionName, out var isTrue))
        {
            isTrue = true;
        }

        return $"Response should {GetBoolRepresentation((bool)isTrue)}have body.";
    }

    private static string GetHasHeaderTestName(IReadOnlyDictionary<string, object> parameters)
        => $"Response should have header with name '{(string)parameters[Consts.TestHasHeaderDirectiveSectionName]}'.";

    private static string GetBoolRepresentation(bool isTrue) => isTrue ? string.Empty : "not ";
}
