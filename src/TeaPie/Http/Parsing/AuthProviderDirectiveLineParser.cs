using System.Text.RegularExpressions;

namespace TeaPie.Http.Parsing;

internal partial class AuthProviderDirectiveLineParser : ILineParser
{
    public bool CanParse(string line, HttpParsingContext context)
        => Regex.IsMatch(line, HttpFileParserConstants.AuthProviderSelectorDirectivePattern);

    public void Parse(string line, HttpParsingContext context)
    {
        var match = Regex.Match(line, HttpFileParserConstants.AuthProviderSelectorDirectivePattern);
        if (match.Success)
        {
            context.AuthProviderName = match.Groups[HttpFileParserConstants.AuthProviderDirectiveSectionName].Value;
        }
        else
        {
            throw new InvalidOperationException(
                $"Unable to parse '{HttpFileParserConstants.AuthProviderDirectiveName}' directive.");
        }
    }
}
