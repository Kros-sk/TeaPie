namespace TeaPie.Http;

internal interface ILineParser
{
    bool CanParse(string line, bool isBody);
    void Parse(string line, HttpParsingContext context);
}
