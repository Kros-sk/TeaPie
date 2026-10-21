namespace TeaPie.Templating;

internal static class FluidExpressionIdentifier
{
    public static bool StartsWithIdentifier(string expression, string identifier)
    {
        if (!expression.StartsWith(identifier, StringComparison.Ordinal))
        {
            return false;
        }

        return expression.Length == identifier.Length || expression[identifier.Length] is '.' or '[' or ' ' or '|' or '\t';
    }
}
