using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace TeaPie;

internal static partial class ExceptionHandler
{
    private static readonly string _messageTemplate =
        "Exception was thrown during execution of '{StepName}'. Error message: {Message}" + Environment.NewLine +
        "Stack trace: {StackTrace}";

    internal static void Handle(Exception ex, Type stepType, ILogger logger)
        => logger.LogError(_messageTemplate,
            ParseStepName(stepType.Name),
            ex.Message,
            ex.StackTrace);

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
