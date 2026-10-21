using System.Text.RegularExpressions;

namespace TeaPie.Templating;

internal static partial class TemplateDetector
{
    public const string SupportedTagNames = "for, if, unless, assign, raw";

    public static bool ContainsTemplateTag(string content)
        => content.Contains("{%", StringComparison.Ordinal) && SupportedTagRegex().IsMatch(content);

    public static bool ContainsTagDelimiter(string content)
        => content.Contains("{%", StringComparison.Ordinal);

    [GeneratedRegex(@"\{%-?\s*(?:for|if|unless|assign|raw)\b")]
    private static partial Regex SupportedTagRegex();
}
