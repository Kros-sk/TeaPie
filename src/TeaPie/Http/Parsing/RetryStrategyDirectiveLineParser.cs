using System.Text.RegularExpressions;

namespace TeaPie.Http.Parsing;

internal class RetryStrategyDirectiveLineParser : ILineParser
{
    public bool CanParse(string line, HttpParsingContext context)
        => Regex.IsMatch(line, HttpFileParserConstants.RetryStrategySelectorDirectivePattern);

    public void Parse(string line, HttpParsingContext context)
    {
        var match = Regex.Match(line, HttpFileParserConstants.RetryStrategySelectorDirectivePattern);
        if (match.Success)
        {
            context.RetryStrategyName = match.Groups[HttpFileParserConstants.RetryStrategyDirectiveParameterName].Value;
        }
        else
        {
            throw new InvalidOperationException(
                $"Unable to parse '{HttpFileParserConstants.RetryStrategyDirectiveFullName}' directive.");
        }
    }
}
