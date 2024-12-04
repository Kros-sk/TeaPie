using System.Text.RegularExpressions;

namespace TeaPie.Http;

internal partial class CommentLineParser : ILineParser
{
    public bool CanParse(string line, HttpParsingContext context)
        => !context.IsBody && line.TrimStart().StartsWith(HttpFileParserConstants.HttpCommentPrefix);

    public void Parse(string line, HttpParsingContext context)
    {
        var match = RequestNameRegex().Match(line);

        if (match.Success)
        {
            context.RequestName = match.Groups[HttpFileParserConstants.RequestNameMetadataGroupName].Value;
        }
    }

    [GeneratedRegex(HttpFileParserConstants.RequestNameMetadataPattern)]
    private static partial Regex RequestNameRegex();
}
