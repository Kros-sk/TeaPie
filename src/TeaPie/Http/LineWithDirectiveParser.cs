namespace TeaPie.Http;

internal class LineWithDirectiveParser : ILineParser
{
    public bool CanParse(string line, HttpParsingContext context)
        => !context.IsBody && line.TrimStart().StartsWith(HttpFileParserConstants.HttpDirectivePrefix);

    public void Parse(string line, HttpParsingContext context) => throw new NotImplementedException();
}
