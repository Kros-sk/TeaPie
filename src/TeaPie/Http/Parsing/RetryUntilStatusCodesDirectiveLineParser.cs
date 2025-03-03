using System.Net;
using System.Text.RegularExpressions;

namespace TeaPie.Http.Parsing;

internal partial class RetryUntilStatusCodesLineParser : ILineParser
{
    public bool CanParse(string line, HttpParsingContext context)
        => Regex.IsMatch(line, HttpFileParserConstants.RetryUntilStatusCodesDirectivePattern);

    public void Parse(string line, HttpParsingContext context)
    {
        var match = Regex.Match(line, HttpFileParserConstants.RetryUntilStatusCodesDirectivePattern);
        if (match.Success)
        {
            var statusCodesText = match.Groups[HttpFileParserConstants.RetryUntilStatusCodesDirectiveParameterName].Value;

            context.RetryUntilStatusCodes = NumberPattern().Matches(statusCodesText)
                .Select(m => (HttpStatusCode)int.Parse(m.Value))
                .ToArray();
        }
        else
        {
            throw new InvalidOperationException(
                $"Unable to parse '{HttpFileParserConstants.RetryUntilStatusCodesDirectiveFullName}' " +
                "if directive doesn't match the structure.");
        }
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex NumberPattern();
}
