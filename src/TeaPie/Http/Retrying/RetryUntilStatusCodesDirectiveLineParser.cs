using System.Net;
using System.Text.RegularExpressions;
using TeaPie.Http.Retrying;

namespace TeaPie.Http.Parsing;

internal partial class RetryUntilStatusCodesLineParser : ILineParser
{
    public bool CanParse(string line, HttpParsingContext context)
        => Regex.IsMatch(line, RetryingDirectives.RetryUntilStatusCodesDirectivePattern);

    public void Parse(string line, HttpParsingContext context)
    {
        var match = Regex.Match(line, RetryingDirectives.RetryUntilStatusCodesDirectivePattern);
        if (match.Success)
        {
            var statusCodesText = match.Groups[RetryingDirectives.RetryUntilStatusCodesDirectiveParameterName].Value;

            context.RetryUntilStatusCodes = NumberPattern().Matches(statusCodesText)
                .Select(m => (HttpStatusCode)int.Parse(m.Value))
                .ToArray();
        }
        else
        {
            throw new InvalidOperationException(
                $"Unable to parse '{RetryingDirectives.RetryUntilStatusCodesDirectiveFullName}' " +
                "if directive doesn't match the structure.");
        }
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex NumberPattern();
}
