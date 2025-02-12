using System.Text.RegularExpressions;

namespace TeaPie.Http.Parsing;

internal partial class CommentLineParser : ILineParser
{
    public bool CanParse(string line, HttpParsingContext context)
    {
        if (context.IsBody)
        {
            return false;
        }

        var trimmed = line.TrimStart();

        return trimmed.StartsWith(HttpFileParserConstants.HttpCommentPrefix) ||
            trimmed.StartsWith(HttpFileParserConstants.HttpCommentAltPrefix);
    }

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
