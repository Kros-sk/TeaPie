using System.Text.RegularExpressions;
using TeaPie.Http;

namespace TeaPie.Variables;

internal partial class VariableNameValidator
{
    public static void Resolve(string? name)
    {
        if (!IsValid(name, out var errors))
        {
            throw new VariableNameViolationException(string.Join(Environment.NewLine, errors));
        }
    }

    private static bool IsValid(string? name, out List<string> validationErrors)
    {
        validationErrors = [];
        if (name == null)
        {
            validationErrors.Add("The variable name can not be null.");
            return false;
        }

        name = name.Trim();
        if (name.Equals(string.Empty))
        {
            validationErrors.Add("The variable name can not be an empty string.");
            return false;
        }

        if (VariableNameRegex().IsMatch(name))
        {
            return true;
        }
        else
        {
            validationErrors
                .Add($"The variable name '{name}' contains invalid characters " +
                "(only characters a-z, A-Z, 0-9 and '-' is allowed).");

            return false;
        }
    }

    [GeneratedRegex(HttpFileParserConstants.VariableNamePattern)]
    private static partial Regex VariableNameRegex();
}
