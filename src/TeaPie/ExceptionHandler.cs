using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace TeaPie;

internal static partial class ExceptionHandler
{
    internal static void Handle(Exception ex, Type stepType, ILogger logger)
    {
        logger.LogError("Exception was thrown during execution of '{StepName}'. Error message: {Message}",
            ParseStepName(stepType.Name),
            ex.Message);

        logger.LogDebug("Stack trace: {StackTrace}", ex.StackTrace);
    }

    public static string ParseStepName(string stepName)
    {
        var words = PascalCaseNamesRegex().Matches(stepName)
            .Cast<Match>()
            .Select(m => m.Value)
            .ToArray();

        return string.Join(' ', words);
    }

    [GeneratedRegex(Constants.PascalCasePattern)]
    private static partial Regex PascalCaseNamesRegex();
}
