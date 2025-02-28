using System.Net;
using System.Text.RegularExpressions;

namespace TeaPie.Http.Parsing;

internal class RetryUntilStatusCodesLineParser : ILineParser
{
    public bool CanParse(string line, HttpParsingContext context)
        => Regex.IsMatch(line, HttpFileParserConstants.RetryUntilStatusCodesDirectivePattern);

    public void Parse(string line, HttpParsingContext context)
    {
        var match = Regex.Match(line, HttpFileParserConstants.RetryUntilStatusCodesDirectivePattern);
        if (match.Success)
        {
            var statusCodesText = match.Groups[HttpFileParserConstants.RetryUntilStatusCodesDirectiveSectionName].Value;

            context.RetryUntilStatusCodes = statusCodesText
                .Split(',')
                .Select(code => code.Trim())
                .Where(code => int.TryParse(code, out _))
                .Select(code => (HttpStatusCode)int.Parse(code))
                .ToList();
        }
        else
        {
            throw new InvalidOperationException($"Unable to parse '{HttpFileParserConstants.RetryUntilStatusCodesDirectiveName}' "
                + "if directive doesn't match the structure.");
        }
    }
}
