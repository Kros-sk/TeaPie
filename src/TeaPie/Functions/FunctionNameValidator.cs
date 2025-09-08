using System.Text.RegularExpressions;
using TeaPie.Http.Parsing;

namespace TeaPie.Functions;

internal partial class FunctionNameValidator
{
    public static void Resolve(string? name)
    {
        if (!IsValid(name, out var errors))
        {
            throw new FunctionNameViolationException(string.Join(Environment.NewLine, errors));
        }
    }

    private static bool IsValid(string? name, out List<string> validationErrors)
    {
        validationErrors = [];
        if (name == null)
        {
            validationErrors.Add("The function name can not be null.");
            return false;
        }

        name = name.Trim();
        if (name.Equals(string.Empty))
        {
            validationErrors.Add("The function name can not be an empty string.");
            return false;
        }

        if (FunctionNameRegex().IsMatch(name))
        {
            return true;
        }
        else
        {
            validationErrors
                .Add($"The function name '{name}' contains invalid characters " +
                "(only characters a-z, A-Z, 0-9 and '-' are allowed).");

            return false;
        }
    }

    [GeneratedRegex(HttpFileParserConstants.FunctionNamePattern)]
    private static partial Regex FunctionNameRegex();
}
