using System.Text.RegularExpressions;
using TeaPie.Testing;

namespace TeaPie.Http.Parsing;

internal partial class TestDirectivesLineParser : ILineParser
{
    private static readonly List<string> _supportedPatterns = [
        HttpFileParserConstants.TestExpectStatusCodesDirectivePattern,
        HttpFileParserConstants.TestHasBodyDirectivePattern,
        HttpFileParserConstants.TestHasHeaderDirectivePattern,
    ];

    public static void RegisterTestDirective(string pattern) => _supportedPatterns.Add(pattern);

    public bool CanParse(string line, HttpParsingContext context)
        => TestDirectivePattern().IsMatch(line);

    public void Parse(string line, HttpParsingContext context)
    {
        foreach (var pattern in _supportedPatterns)
        {
            var match = Regex.Match(line, pattern);
            if (match.Success)
            {
                ParseDirective(match, context);
                return;
            }
        }

        throw new InvalidOperationException($"Unable to parse any retry explicit property directive on line '{line}'.");
    }

    private static void ParseDirective(
        Match match,
        HttpParsingContext context)
    {
        var directiveName = match.Groups[HttpFileParserConstants.TestDirectiveSectionName].Value;
        var parameters = match.Groups.Values.Select(g => g.Value).ToArray();

        context.RegiterTest(new TestDescription(directiveName, TestType.Custom, parameters));
    }

    [GeneratedRegex(HttpFileParserConstants.TestDirectivePattern)]
    private static partial Regex TestDirectivePattern();
}
