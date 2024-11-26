namespace TeaPie.Http;

internal class RequestSeparatorParser : ILineParser
{
    public bool CanParse(string line, HttpParsingContext context)
        => line.TrimStart().StartsWith(HttpFileParserConstants.HttpRequestSeparatorDirective);

    public void Parse(string line, HttpParsingContext context) { }
}
