using System.Text.RegularExpressions;

namespace TeaPie.Templating;

internal static partial class FluidParseErrorMapper
{
    public static string RemapToOriginal(
        string parseError, string originalContent, string transformedContent, IReadOnlyList<TransformedTextSpan> map)
    {
        var match = PositionRegex().Match(parseError);

        if (!match.Success)
        {
            return parseError;
        }

        var transformedLine = int.Parse(match.Groups["line"].Value);
        var transformedColumn = int.Parse(match.Groups["column"].Value);

        var transformedOffset = ToOffset(transformedContent, transformedLine, transformedColumn);
        var originalOffset = MapToOriginalOffset(transformedOffset, map);
        var (originalLine, originalColumn) = ToLineColumn(originalContent, originalOffset);
        var originalSourceLine = GetLine(originalContent, originalLine);

        return $"{match.Groups["message"].Value} at ({originalLine}:{originalColumn})\nSource:\n{originalSourceLine}";
    }

    private static int MapToOriginalOffset(int transformedOffset, IReadOnlyList<TransformedTextSpan> map)
    {
        foreach (var span in map)
        {
            if (transformedOffset >= span.TransformedStart && transformedOffset < span.TransformedEnd)
            {
                return span.IsEdited
                    ? span.OriginalStart
                    : span.OriginalStart + (transformedOffset - span.TransformedStart);
            }
        }

        return map.Count > 0 ? map[^1].OriginalEnd : transformedOffset;
    }

    private static int ToOffset(string text, int line, int column)
    {
        var currentLine = 1;
        var offset = 0;

        while (currentLine < line)
        {
            var newlineIndex = text.IndexOf('\n', offset);
            if (newlineIndex < 0)
            {
                return text.Length;
            }

            offset = newlineIndex + 1;
            currentLine++;
        }

        return Math.Min(offset + (column - 1), text.Length);
    }

    private static (int Line, int Column) ToLineColumn(string text, int offset)
    {
        offset = Math.Clamp(offset, 0, text.Length);
        var line = 1;
        var lineStart = 0;

        for (var i = 0; i < offset; i++)
        {
            if (text[i] == '\n')
            {
                line++;
                lineStart = i + 1;
            }
        }

        return (line, offset - lineStart + 1);
    }

    private static string GetLine(string text, int lineNumber)
    {
        var start = 0;
        var currentLine = 1;

        while (currentLine < lineNumber)
        {
            var newlineIndex = text.IndexOf('\n', start);
            if (newlineIndex < 0)
            {
                return string.Empty;
            }

            start = newlineIndex + 1;
            currentLine++;
        }

        var end = text.IndexOf('\n', start);
        var line = end < 0 ? text[start..] : text[start..end];
        return line.TrimEnd('\r');
    }

    [GeneratedRegex(FluidMessageFormats.ParseErrorPositionPattern, RegexOptions.Singleline)]
    private static partial Regex PositionRegex();
}
