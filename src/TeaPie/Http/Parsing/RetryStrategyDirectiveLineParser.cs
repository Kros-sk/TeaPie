using System.Text.RegularExpressions;
using TeaPie.Http.Retrying;

namespace TeaPie.Http.Parsing;

internal class RetryStrategyDirectiveLineParser : ILineParser
{
    public bool CanParse(string line, HttpParsingContext context)
        => Regex.IsMatch(line, RetryingDirectives.RetryStrategySelectorDirectivePattern);

    public void Parse(string line, HttpParsingContext context)
    {
        var match = Regex.Match(line, RetryingDirectives.RetryStrategySelectorDirectivePattern);
        if (match.Success)
        {
            context.RetryStrategyName = match.Groups[RetryingDirectives.RetryStrategyDirectiveParameterName].Value;
        }
        else
        {
            throw new InvalidOperationException(
                $"Unable to parse '{RetryingDirectives.RetryStrategyDirectiveFullName}' directive.");
        }
    }
}
