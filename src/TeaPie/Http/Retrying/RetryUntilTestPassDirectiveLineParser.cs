using System.Text.RegularExpressions;
using TeaPie.Http.Parsing;

namespace TeaPie.Http.Retrying;

internal partial class RetryUntilTestPassDirectiveLineParser : ILineParser
{
    public bool CanParse(string line, HttpParsingContext context)
        => Regex.IsMatch(line, RetryingDirectives.RetryUntilTestPassDirectivePattern);

    public void Parse(string line, HttpParsingContext context)
    {
        var match = Regex.Match(line, RetryingDirectives.RetryUntilTestPassDirectivePattern);
        if (match.Success)
        {
            var testName = match.Groups[RetryingDirectives.RetryUntilTestPassDirectiveParameterName].Value;
            context.RetryUntilTestPassTestName = testName.Trim();
        }
        else
        {
            throw new InvalidOperationException(
                $"Unable to parse '{RetryingDirectives.RetryUntilTestPassDirectiveFullName}' " +
                "if directive doesn't match the structure.");
        }
    }
}
