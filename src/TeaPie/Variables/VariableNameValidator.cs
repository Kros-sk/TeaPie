using System.Text.RegularExpressions;
using TeaPie.Exceptions;
using TeaPie.Parsing;

namespace TeaPie.Variables;

internal partial class VariableNameValidator
{
    public static void Resolve(object? name)
    {
        if (!IsValid(name, out var errors))
        {
            throw new VariableNameViolationException(string.Join(Environment.NewLine, errors));
        }
    }

    private static bool IsValid(object? value, out List<string> validationErrors)
    {
        validationErrors = [];
        if (value == null)
        {
            validationErrors.Add("The variable name can not be null.");
            return false;
        }

        if (value is not string name)
        {
            validationErrors.Add("The variable name has to be in string format.");
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
                .Add($"The variable name '{name}' contains invalid characters (only a-z, A-Z, 0-9 and '-' is allowed).");

            return false;
        }
    }

    [GeneratedRegex(ParsingConstants.VariableNamePattern)]
    private static partial Regex VariableNameRegex();
}
