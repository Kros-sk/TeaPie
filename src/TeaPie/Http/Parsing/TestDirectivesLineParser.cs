using System.Text.RegularExpressions;
using TeaPie.Testing;

namespace TeaPie.Http.Parsing;

internal class TestDirectivesLineParser : ILineParser
{
    private static readonly List<string> _supportedDirectivesPatterns =
        DefaultDirectivesProvider.GetDefaultTestDirectives().ConvertAll(x => x.DirectivePattern);

    public static void RegisterTestDirective(string directivePattern) => _supportedDirectivesPatterns.Add(directivePattern);

    public bool CanParse(string line, HttpParsingContext context)
        => _supportedDirectivesPatterns.Any(p => Regex.IsMatch(line, p));

    public void Parse(string line, HttpParsingContext context)
    {
        foreach (var pattern in _supportedDirectivesPatterns)
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
        var directiveName = match.Groups[TestDirectives.TestDirectiveParameterName].Value;
        var parameters = match.Groups.Keys
            .Select(key => new KeyValuePair<string, string>(key, match.Groups[key].Value))
            .ToDictionary();

        context.RegiterTest(new TestDescription(directiveName, parameters));
    }
}
