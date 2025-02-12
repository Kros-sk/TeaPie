namespace TeaPie.Http.Parsing;

internal interface ILineParser
{
    bool CanParse(string line, HttpParsingContext context);
    void Parse(string line, HttpParsingContext context);
}
