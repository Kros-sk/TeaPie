
namespace TeaPie.Http.Parsing;

internal class RetryDirectivesLineParser : ILineParser
{
    private readonly IEnumerable<ILineParser> _parsers = [
        new RetryStrategyDirectiveLineParser(),
        new RetryUntilStatusCodesLineParser(),
        new RetryExplicitPropertiesDirectiveLineParser()
    ];

    public bool CanParse(string line, HttpParsingContext context)
        => !context.IsBody && line.TrimStart().StartsWith(HttpFileParserConstants.HttpDirectivePrefix);

    public void Parse(string line, HttpParsingContext context)
    {
        foreach (var parser in _parsers)
        {
            if (parser.CanParse(line, context))
            {
                parser.Parse(line, context);
                break;
            }
        }
    }
}
