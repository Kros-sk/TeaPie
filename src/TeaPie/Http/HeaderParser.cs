namespace TeaPie.Http;

internal class HeaderParser : ILineParser
{
    public bool CanParse(string line, HttpParsingContext context)
        => !context.IsBody && line.Contains(HttpFileParserConstants.HttpHeaderSeparator);

    public void Parse(string line, HttpParsingContext context)
    {
        var parts = line.Split(HttpFileParserConstants.HttpHeaderSeparator, 2);
        if (parts.Length == 2)
        {
            var name = parts[0].Trim();
            var value = parts[1].Trim();

            ResolveHeaders(context, name, value);
        }
    }

    private static void ResolveHeaders(HttpParsingContext context, string name, string value)
    {
        if (HttpFileParserConstants.SpecialHeaders.Contains(name))
        {
            context.SpecialHeaders.Add(name, value);
        }
        else if (!context.Headers.TryAddWithoutValidation(name, value))
        {
            throw new InvalidOperationException($"Unable to resolve header '{name} : {value}'");
        }
    }
}
