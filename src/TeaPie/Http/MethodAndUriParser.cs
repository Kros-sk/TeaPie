namespace TeaPie.Http;

internal class MethodAndUriParser : ILineParser
{
    public bool CanParse(string line, HttpParsingContext context)
        => !context.IsMethodAndUriResolved && !context.IsBody && !string.IsNullOrWhiteSpace(line);

    public void Parse(string line, HttpParsingContext context)
    {
        var parts = line.TrimStart().Split(' ', 2);
        if (parts.Length < 2)
        {
            throw new InvalidOperationException("Invalid request line.");
        }

        if (!HttpFileParserConstants.HttpMethodsMap.TryGetValue(parts[0], out var method))
        {
            throw new InvalidOperationException($"Unsupported HTTP method '{parts[0]}'.");
        }

        context.Method = method;
        context.RequestUri = parts[1].Trim();
        context.IsMethodAndUriResolved = true;
    }
}
