using System.Net;
using System.Text.RegularExpressions;

namespace TeaPie.Http;

internal partial class RetryUntilStatusCodesLineParser : ILineParser
{
    public bool CanParse(string line, HttpParsingContext context)
        => RetryUntilStatusCodesRegex().IsMatch(line);

    public void Parse(string line, HttpParsingContext context)
    {
        var match = RetryUntilStatusCodesRegex().Match(line);
        if (match.Success)
        {
            var statusCodesText = match.Groups["StatusCodes"].Value;

            context.RetryUntilStatusCodes = statusCodesText
                .Split(',')
                .Select(code => code.Trim())
                .Where(code => int.TryParse(code, out _))
                .Select(code => (HttpStatusCode)int.Parse(code))
                .ToList();
        }
        else
        {
            throw new InvalidOperationException($"Unable to parse {HttpFileParserConstants.RetryUntilStatusCodesDirectiveName} "
                + "if directive doesn't match the structure.");
        }
    }

    [GeneratedRegex(
        HttpFileParserConstants.RetryUntilStatusCodesDirectivePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled, "sk-SK")]
    private static partial Regex RetryUntilStatusCodesRegex();
}
