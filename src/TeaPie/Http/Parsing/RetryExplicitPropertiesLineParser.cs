using Polly;
using Polly.Retry;
using System.Text.RegularExpressions;
using TeaPie.Http.Retrying;

namespace TeaPie.Http.Parsing;

internal class RetryExplicitPropertiesDirectiveLineParser : ILineParser
{
    private readonly IReadOnlyDictionary<string, Action<Match, HttpParsingContext>> _strategies =
        new Dictionary<string, Action<Match, HttpParsingContext>>()
        {
           { RetryingDirectives.RetryMaxAttemptsDirectivePattern, ParseMaxAttemptsDirective},
           { RetryingDirectives.RetryBackoffTypeDirectivePattern, ParseBackoffTypeDirective},
           { RetryingDirectives.RetryMaxDelayDirectivePattern, ParseMaxDelayDirective}
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

    private static void ParseMaxAttemptsDirective(Match match, HttpParsingContext context)
        => ParseDirective(
            match,
            context,
            "MaxAttempts",
            (context, value) => context.ExplicitRetryStrategy!.MaxRetryAttempts = int.Parse(value));

    private static void ParseBackoffTypeDirective(Match match, HttpParsingContext context)
        => ParseDirective(
            match,
            context,
            "BackoffType",
            (context, value) => context.ExplicitRetryStrategy!.BackoffType = Enum.Parse<DelayBackoffType>(value, true));

    private static void ParseMaxDelayDirective(Match match, HttpParsingContext context)
        => ParseDirective(
            match,
            context,
            "MaxDelay",
            (context, value) => context.ExplicitRetryStrategy!.MaxDelay = TimeSpan.Parse(value));

    private static void ParseDirective(
        Match match,
        HttpParsingContext context,
        string sectionName,
        Action<HttpParsingContext, string> assignFunction)
    {
        var value = match.Groups[sectionName].Value;
        context.ExplicitRetryStrategy ??= new RetryStrategyOptions<HttpResponseMessage>();

        assignFunction(context, value);
    }
}
