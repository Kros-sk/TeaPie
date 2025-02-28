using System.Text.RegularExpressions;
using TeaPie.Testing;
using static Xunit.Assert;
using Consts = TeaPie.Http.Parsing.HttpFileParserConstants;

namespace TeaPie.Http.Parsing;

internal class TestDirectivesLineParser : ILineParser
{
    private static readonly List<TestDirective> _supportedDirectives = GetDefaultDirectives();

    public static void RegisterTestDirective(string pattern) => _supportedDirectives.Add(pattern);

    public bool CanParse(string line, HttpParsingContext context)
        => _supportedDirectives.Any(p => Regex.IsMatch(line, p));

    public void Parse(string line, HttpParsingContext context)
    {
        foreach (var pattern in _supportedDirectives)
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
        var directiveName = match.Groups[Consts.TestDirectiveSectionName].Value;
        var parameters = match.Groups.Keys
            .Select(key => new KeyValuePair<string, object>(key, match.Groups[key].Value))
            .ToDictionary();

        context.RegiterTest(new TestDescription(directiveName, parameters));
    }
}
