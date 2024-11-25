namespace TeaPie.Http;

internal class HeaderParser : ILineParser
{
    public bool CanParse(string line, bool isBody) => !isBody && line.Contains(HttpFileParserConstants.HttpHeaderSeparator);

    public void Parse(string line, HttpParsingContext context)
    {
        var parts = line.Split(HttpFileParserConstants.HttpHeaderSeparator, 2);
        if (parts.Length == 2)
        {
            context.Headers.TryAddWithoutValidation(parts[0].Trim(), parts[1].Trim());
        }
    }
}
