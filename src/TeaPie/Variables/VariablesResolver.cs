﻿using System.Text.RegularExpressions;
using TeaPie.Http;
using TeaPie.Http.Parsing;

namespace TeaPie.Variables;

internal interface IVariablesResolver
{
    string ResolveVariablesInLine(string line, RequestExecutionContext requestExecutionContext);
}

internal partial class VariablesResolver(IVariables variables, IServiceProvider serviceProvider) : IVariablesResolver
{
    private readonly IVariables _variables = variables;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public string ResolveVariablesInLine(string line, RequestExecutionContext requestExecutionContext)
        => VariableNotationPatternRegex().Replace(line, match =>
        {
            var variableName = match.Groups[1].Value;

            if (RequestVariablesResolver.IsRequestVariable(variableName))
            {
                return ResolveRequestVariable(variableName, requestExecutionContext).Result;
            }

            if (_variables.ContainsVariable(variableName))
            {
                var variableValue = _variables.GetVariable<object>(variableName, default);
                return variableValue?.ToString() ?? "null";
            }

            throw new InvalidOperationException($"Variable '{variableName}' was not found.");
        });

    private async Task<string> ResolveRequestVariable(string variableName, RequestExecutionContext requestExecutionContext)
    {
        if (RequestVariablesResolver.TryGetVariableDescription(variableName, out var descriptor))
        {
            var requestVariableResolver = new RequestVariablesResolver(descriptor, _serviceProvider);
            return await requestVariableResolver.Resolve(requestExecutionContext);
        }

        return variableName;
    }

    [GeneratedRegex(HttpFileParserConstants.VariableNotationPattern)]
    private static partial Regex VariableNotationPatternRegex();
}
