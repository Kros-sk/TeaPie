namespace TeaPie.Templating;

internal static class FluidExpressionIdentifier
{
    public const string Pattern = "[A-Za-z0-9_][A-Za-z0-9_-]*";

    public static bool StartsWithIdentifier(string expression, string identifier)
    {
        if (!expression.StartsWith(identifier, StringComparison.Ordinal))
        {
            return false;
        }

        return expression.Length == identifier.Length || expression[identifier.Length] is '.' or '[' or ' ' or '|' or '\t';
    }

    public static bool ReferencesIdentifier(string expression, string identifier)
    {
        var index = 0;

        while (index < expression.Length)
        {
            var current = expression[index];

            if (current is '"' or '\'')
            {
                var closingIndex = expression.IndexOf(current, index + 1);
                if (closingIndex < 0)
                {
                    return false;
                }

                index = closingIndex + 1;
                continue;
            }

            if (!IsIdentifierCharacter(current))
            {
                index++;
                continue;
            }

            var start = index;
            while (index < expression.Length && IsIdentifierCharacter(expression[index]))
            {
                index++;
            }

            if (index - start == identifier.Length
                && string.CompareOrdinal(expression, start, identifier, 0, identifier.Length) == 0
                && IsVariableStart(expression, start)
                && (index == expression.Length || expression[index] is '.' or '[' or '|' or ',' or ']' or ')'
                    || char.IsWhiteSpace(expression[index])))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsIdentifierCharacter(char character)
        => char.IsAsciiLetterOrDigit(character) || character is '_' or '-';

    private static bool IsVariableStart(string expression, int start)
    {
        if (start > 0 && !(expression[start - 1] is ':' or ',' or '[' or '(' || char.IsWhiteSpace(expression[start - 1])))
        {
            return false;
        }

        var previous = start - 1;
        while (previous >= 0 && char.IsWhiteSpace(expression[previous]))
        {
            previous--;
        }

        return previous < 0 || expression[previous] != '|';
    }
}
