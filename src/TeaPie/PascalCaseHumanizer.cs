using System.Text.RegularExpressions;

namespace TeaPie;

internal static partial class PascalCaseHumanizer
{
    public static string ParsePascalCase(this string expression)
    {
        var words = PascalCaseNamesRegex().Matches(expression)
            .Cast<Match>()
            .Select(m => m.Value)
            .ToArray();

        return string.Join(' ', words);
    }

    [GeneratedRegex(Constants.PascalCasePattern)]
    private static partial Regex PascalCaseNamesRegex();
}
