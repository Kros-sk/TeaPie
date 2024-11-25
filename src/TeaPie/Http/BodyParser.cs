namespace TeaPie.Http;

internal class BodyParser : ILineParser
{
    public bool CanParse(string line, bool isBody) => isBody;

    public void Parse(string line, HttpParsingContext context) => context.BodyBuilder.AppendLine(line);
}
