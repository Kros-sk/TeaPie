namespace TeaPie.Http;

internal interface ILineParser
{
    bool CanParse(string line, HttpParsingContext context);
    void Parse(string line, HttpParsingContext context);
}
