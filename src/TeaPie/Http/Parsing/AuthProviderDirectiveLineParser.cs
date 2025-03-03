using System.Text.RegularExpressions;
using TeaPie.Http.Auth;

namespace TeaPie.Http.Parsing;

internal class AuthProviderDirectiveLineParser : ILineParser
{
    public bool CanParse(string line, HttpParsingContext context)
        => Regex.IsMatch(line, AuthDirectives.AuthProviderSelectorDirectivePattern);

    public void Parse(string line, HttpParsingContext context)
    {
        var match = Regex.Match(line, AuthDirectives.AuthProviderSelectorDirectivePattern);
        if (match.Success)
        {
            context.AuthProviderName = match.Groups[AuthDirectives.AuthProviderDirectiveParameterName].Value;
        }
        else
        {
            throw new InvalidOperationException(
                $"Unable to parse '{AuthDirectives.AuthProviderDirectiveFullName}' directive.");
        }
    }
}
