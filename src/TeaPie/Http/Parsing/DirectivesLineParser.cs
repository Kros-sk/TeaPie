namespace TeaPie.Http.Parsing;

internal class DirectivesLineParser : ILineParser
{
    private readonly IEnumerable<ILineParser> _parsers = [
        new AuthProviderDirectiveLineParser(),
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
                return;
            }
        }

        throw new InvalidOperationException("Unable to parse any of supported directive.");
    }
}
