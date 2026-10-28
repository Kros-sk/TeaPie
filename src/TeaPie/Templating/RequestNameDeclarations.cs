using System.Text.RegularExpressions;
using TeaPie.Http.Parsing;

namespace TeaPie.Templating;

internal readonly record struct RequestNameDeclaration(string Name, int Index, bool IsLiteral);

internal static partial class RequestNameDeclarations
{
    public static IEnumerable<RequestNameDeclaration> Find(string content)
    {
        var lineStart = 0;

        foreach (var rawLine in content.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');
            var trimmed = line.TrimStart();

            if (trimmed.StartsWith(HttpFileParserConstants.HttpCommentPrefix)
                || trimmed.StartsWith(HttpFileParserConstants.HttpCommentAltPrefix))
            {
                var match = RequestNameRegex().Match(line);
                if (match.Success)
                {
                    var end = match.Index + match.Length;
                    yield return new RequestNameDeclaration(
                        match.Groups[HttpFileParserConstants.RequestNameMetadataGroupName].Value,
                        lineStart,
                        end == line.Length || line[end] != '{');
                }
            }

            lineStart += rawLine.Length + 1;
        }
    }

    public static Dictionary<string, int> Count(string content)
        => Find(content)
            .GroupBy(declaration => declaration.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);

    [GeneratedRegex(
        HttpFileParserConstants.RequestNameMetadataPattern, RegexOptions.None, TemplatingLimits.RegexTimeoutMilliseconds)]
    private static partial Regex RequestNameRegex();
}
