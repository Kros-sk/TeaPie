using System.Text.RegularExpressions;
using TeaPie.Http;

namespace TeaPie.Variables;

internal interface IVariablesResolver
{
    string ResolveVariablesInLine(string line);
}

internal partial class VariablesResolver(IVariables variables) : IVariablesResolver
{
    private readonly IVariables _variables = variables;

    public string ResolveVariablesInLine(string line)
        => VariableNotationPatternRegex().Replace(line, match =>
        {
            var variableName = match.Groups[1].Value;
            if (_variables.ContainsVariable(variableName))
            {
                var variableValue = _variables.GetVariable<object>(variableName, default);
                return variableValue?.ToString() ?? "null";
            }

            throw new InvalidOperationException($"Variable '{{{variableName}}}' was not found.");
        });

    [GeneratedRegex(HttpFileParserConstants.VariableNotationPattern)]
    private static partial Regex VariableNotationPatternRegex();
}
