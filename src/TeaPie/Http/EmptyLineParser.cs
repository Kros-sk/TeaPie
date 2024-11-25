namespace TeaPie.Http;

internal class EmptyLineParser : ILineParser
{
    public bool CanParse(string line, bool isBody) => string.IsNullOrWhiteSpace(line);

    public void Parse(string line, HttpParsingContext context) => context.IsBody = true;
}
