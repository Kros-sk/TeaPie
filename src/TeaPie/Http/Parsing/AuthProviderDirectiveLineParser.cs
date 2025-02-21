using System.Text.RegularExpressions;

namespace TeaPie.Http.Parsing;

internal partial class AuthProviderDirectiveLineParser : ILineParser
{
    public bool CanParse(string line, HttpParsingContext context)
        => AuthProviderSelectorRegex().IsMatch(line);

    public void Parse(string line, HttpParsingContext context)
    {
        var match = AuthProviderSelectorRegex().Match(line);
        if (match.Success)
        {
            context.AuthProviderName = match.Groups["AuthProvider"].Value;
        }
        else
        {
            throw new InvalidOperationException(
                $"Unable to parse '{HttpFileParserConstants.AuthProviderDirectiveName}' directive.");
        }
    }

    [GeneratedRegex(
        HttpFileParserConstants.AuthProviderSelectorDirectivePattern,
        RegexOptions.IgnoreCase | RegexOptions.Compiled, "sk-SK")]
    private static partial Regex AuthProviderSelectorRegex();
}
