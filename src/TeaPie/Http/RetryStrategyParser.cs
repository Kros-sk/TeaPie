using System.Text.RegularExpressions;

namespace TeaPie.Http;

internal partial class RetryStrategyParser : ILineParser
{
    public bool CanParse(string line, HttpParsingContext context)
        => RetryStrategySelectorRegex().IsMatch(line);

    public void Parse(string line, HttpParsingContext context)
    {
        var match = RetryStrategySelectorRegex().Match(line);
        if (match.Success)
        {
            context.RetryStrategyName = match.Groups["StrategyName"].Value;
        }
        else
        {
            throw new InvalidOperationException(
                $"Unable to parse {HttpFileParserConstants.RetryStrategyDirectiveName} directive.");
        }
    }

    [GeneratedRegex(
        HttpFileParserConstants.RetryStrategySelectorDirectivePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled, "sk-SK")]
    private static partial Regex RetryStrategySelectorRegex();
}
