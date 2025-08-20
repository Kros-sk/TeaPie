using System.CommandLine.Parsing;
using System.Text.RegularExpressions;
using TeaPie.Http;
using TeaPie.Http.Parsing;

namespace TeaPie.Functions;

internal interface IFunctionsResolver
{
    string ResolveFunctionsInLine(string line, RequestExecutionContext requestExecutionContext);
}

internal partial class FunctionsResolver(IFunctions functions) : IFunctionsResolver
{
    private readonly IFunctions _functions = functions;

    public string ResolveFunctionsInLine(string line, RequestExecutionContext requestExecutionContext)
        => FunctionNotationPatternRegex().Replace(line, match =>
        {
            string input = match.Groups[1].Value;
            IEnumerable<string> tokens = CommandLineParser.SplitCommandLine(input);

            var functionName = tokens.First();
            string[] args = [.. tokens.Skip(1)];

            if (_functions.Contains(functionName))
            {
                object? result;

                if (!args.Any())
                {
                    result = _functions.Execute<object>(functionName);
                }
                else
                {
                    result = _functions.Execute<object>(functionName, args);
                }
                return result?.ToString() ?? "null";
            }

            throw new InvalidOperationException($"Function '{functionName}' was not found.");
        });

    [GeneratedRegex(HttpFileParserConstants.FunctionNotationPattern)]
    private static partial Regex FunctionNotationPatternRegex();
}
