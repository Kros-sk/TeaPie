namespace TeaPie.Http;

internal class CommentLineParser : ILineParser
{
    public bool CanParse(string line, HttpParsingContext context)
        => !context.IsBody && line.TrimStart().StartsWith(HttpFileParserConstants.HttpCommentPrefix);

    public void Parse(string line, HttpParsingContext context) { }
}
