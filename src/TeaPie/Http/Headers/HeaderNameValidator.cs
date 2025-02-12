using System.Data;
using System.Text.RegularExpressions;
using TeaPie.Http.Parsing;

namespace TeaPie.Http.Headers;

internal partial class HeaderNameValidator
{
    public static void CheckName(string headerName)
    {
        if (!HeaderNameRegex().Match(headerName).Success)
        {
            throw new SyntaxErrorException($"Header name '{headerName}' is invalid.");
        }
    }

    public static void CheckValue(string headerValue)
    {
        if (!HeaderValueRegex().Match(headerValue).Success)
        {
            throw new SyntaxErrorException($"Header value '{headerValue}' is invalid.");
        }
    }

    internal static void CheckHeader(string name, string value)
    {
        CheckName(name);
        CheckValue(value);
    }

    [GeneratedRegex(HttpFileParserConstants.HeaderNamePattern)]
    private static partial Regex HeaderNameRegex();

    [GeneratedRegex(HttpFileParserConstants.HeaderValuePattern)]
    private static partial Regex HeaderValueRegex();
}
