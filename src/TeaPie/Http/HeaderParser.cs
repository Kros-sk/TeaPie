using TeaPie.Http.Headers;

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

            ResolveHeader(context, name, value);
        }
    }

    private static void ResolveHeader(HttpParsingContext context, string name, string value)
    {
        HeaderNameValidator.CheckHeader(name, value);
        context.AddHeader(name, value);
    }
}
