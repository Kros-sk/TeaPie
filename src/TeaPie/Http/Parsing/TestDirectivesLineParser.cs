using Polly;
using Polly.Retry;
using System.Net;
using System.Text.RegularExpressions;
using TeaPie.Testing;

namespace TeaPie.Http.Parsing;

internal class TestDirectivesLineParser : ILineParser
{
    private readonly IReadOnlyDictionary<string, Action<Match, HttpParsingContext>> _strategies =
    new Dictionary<string, Action<Match, HttpParsingContext>>()
    {
        { HttpFileParserConstants.TestExpectStatusCodesDirectivePattern, ParseExpectStatusCodes},
        { HttpFileParserConstants.TestHasBodyDirectivePattern, ParseHasHeader},
        { HttpFileParserConstants.TestHasHeaderDirectivePattern, ParseHasBody}
    };

    public bool CanParse(string line, HttpParsingContext context)
        => _strategies.Keys.Any(p => Regex.IsMatch(line, p));

    public void Parse(string line, HttpParsingContext context)
    {
        var parsed = false;
        foreach (var strategy in _strategies)
        {
            var match = Regex.Match(line, strategy.Key);
            if (match.Success)
            {
                strategy.Value(match, context);
                parsed = true;
                break;
            }
        }

        if (!parsed)
        {
            throw new InvalidOperationException($"Unable to parse any retry explicit property directive on line '{line}'.");
        }
    }

    private static void ParseExpectStatusCodes(Match match, HttpParsingContext context)
        => ParseDirective(
            match,
            context,
            HttpFileParserConstants.TestExpectStatusCodesSectionName,
            (context, value) => context.ScheduleTest(GetExpectStatusCodeTestDescription(value)));

    private static PredefinedTestDescription GetExpectStatusCodeTestDescription(string statusCodesText)
        => new(PredefinedTestType.ExpectStatusCodes, ParseStatusCodes(statusCodesText));

    private static int[] ParseStatusCodes(string statusCodesText)
        => statusCodesText
            .Split(',')
            .Select(code => code.Trim())
            .Where(code => int.TryParse(code, out _))
            .Select(int.Parse)
            .ToArray();

    private static void ParseHasHeader(Match match, HttpParsingContext context)
        => ParseDirective(
            match,
            context,
            HttpFileParserConstants.TestHasHeaderDirectiveSectionName,
            (context, value) => context.ExplicitRetryStrategy!.BackoffType = Enum.Parse<DelayBackoffType>(value, true));

    private static void ParseHasBody(Match match, HttpParsingContext context)
        => ParseDirective(
            match,
            context,
            HttpFileParserConstants.TestHasBodyDirectiveSectionName,
            (context, value) => context.ScheduleTest(null!),
            true.ToString());

    private static void ParseDirective(
        Match match,
        HttpParsingContext context,
        string sectionName,
        Action<HttpParsingContext, string> assignFunction,
        string defaultValue = "")
    {
        var value = defaultValue;
        if (match.Groups.ContainsKey(sectionName))
        {
            value = match.Groups[sectionName].Value;
        }

        context.ExplicitRetryStrategy ??= new RetryStrategyOptions<HttpResponseMessage>();

        assignFunction(context, value);
    }
}
