namespace TeaPie.Http;

internal class EmptyLineParser : ILineParser
{
    public bool CanParse(string line, HttpParsingContext context) => string.IsNullOrWhiteSpace(line);

    public void Parse(string line, HttpParsingContext context)
    {
        if (context.IsMethodAndUriResolved)
        {
            context.IsBody = true;
        }
    }
}
