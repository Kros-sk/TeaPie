namespace TeaPie.Logging;

internal static class TreeConsoleWriter
{
    internal const string VerticalBar = "│  ";
    private const string StartCorner = "┌──";
    private const string EndCorner = "└──";
    internal const string TimestampFormat = "HH:mm:ss";
    internal const string DefaultOutputTemplate = $"[{{Timestamp:{TimestampFormat}}} {{Level:u3}}] {{Message:lj}}{{NewLine}}{{Exception}}";
    internal const int ExpectedLevelLength = 3;
    internal const int ExpectedTimestampLength = 8;
    internal const int MinHeaderLengthForLevel = ExpectedLevelLength + 1;
    internal const int BracketStartIndex = 0;
    internal const int TimestampStartIndex = BracketStartIndex + 1;
    internal const int LevelStartIndex = TimestampStartIndex + ExpectedTimestampLength + 1;
    internal const int BracketEndIndex = LevelStartIndex + ExpectedLevelLength;
    internal const int ExpectedMinHeaderLength = BracketEndIndex + 1;

    internal static void WriteOpening(int depth, DateTimeOffset timestamp, string levelShort)
        => WriteScopeBracket(StartCorner, depth, timestamp, levelShort);

    internal static void WriteClosing(int depth, DateTimeOffset timestamp, string levelShort)
        => WriteScopeBracket(EndCorner, depth, timestamp, levelShort);

    private static void WriteScopeBracket(string corner, int depth, DateTimeOffset timestamp, string levelShort)
    {
        var prefix = BuildPrefix(depth - 1);
        var headerTemplate = BuildHeader(timestamp, levelShort) + " ";
        var emptyHeader = new string(' ', headerTemplate.Length);

        WriteIndent(emptyHeader, prefix, null);
        TreeConsoleFormatter.WriteColorText(corner, TreeConsoleFormatter.GetTreeColor(depth - 1));
        Console.Out.WriteLine();
    }

    internal static string BuildPrefix(int repeat)
        => repeat <= 0 ? string.Empty : string.Concat(Enumerable.Repeat(VerticalBar, repeat));

    private static string BuildHeader(DateTimeOffset timestamp, string levelShort)
        => $"[{timestamp.ToString(TimestampFormat)} {levelShort}]";

    internal static void WriteLogMessage(string rendered, string prefix)
    {
        var lines = rendered.TrimEnd('\r', '\n').Split('\n');
        var firstLine = lines[0].TrimEnd('\r');
        var headerEnd = firstLine.IndexOf("] ");
        var headerWidth = headerEnd >= 0 ? headerEnd + 2 : 0;

        var header = firstLine[..headerWidth];
        var levelShort = headerWidth >= MinHeaderLengthForLevel
            ? firstLine.Substring(headerEnd - ExpectedLevelLength, ExpectedLevelLength)
            : "UNK";

        var emptyHeader = new string(' ', headerWidth);
        var message = TreeConsoleFormatter.PrependSymbol(firstLine[headerWidth..]);

        if (string.IsNullOrWhiteSpace(message))
        {
            WriteWrappedLine(emptyHeader, prefix, message, null);
        }
        else
        {
            WriteWrappedLine(header, prefix, message, levelShort);
        }

        for (var i = 1; i < lines.Length; i++)
        {
            WriteWrappedLine(emptyHeader, prefix, lines[i].TrimEnd('\r'), levelShort);
        }
    }

    private static void WriteWrappedLine(string header, string prefix, string message, string? levelShort)
    {
        var width = GetConsoleWidth();
        var indentLength = header.Length + prefix.Length;
        var printHeader = header;
        var printLevel = levelShort;

        if (width <= 0 || (indentLength + message.Length) <= width)
        {
            WriteIndent(printHeader, prefix, printLevel);
            TreeConsoleFormatter.WriteMessageWithSymbolColor(message);
            return;
        }

        var chunkLength = Math.Max(1, width - indentLength);
        var remaining = message;
        var emptyHeader = new string(' ', header.Length);

        while (remaining.Length > 0)
        {
            WriteIndent(printHeader, prefix, printLevel);

            var currentChunkSize = Math.Min(remaining.Length, chunkLength);
            var chunk = remaining[..currentChunkSize];
            TreeConsoleFormatter.WriteMessageWithSymbolColor(chunk);
            remaining = remaining[currentChunkSize..];

            printHeader = emptyHeader;
            printLevel = null;
        }
    }

    private static void WriteIndent(string header, string prefix, string? levelShort)
    {
        TreeConsoleFormatter.WriteColorizedHeader(header, levelShort);

        var depth = string.IsNullOrEmpty(prefix) ? 0 : prefix.Length / VerticalBar.Length;
        for (var i = 0; i < depth; i++)
        {
            TreeConsoleFormatter.WriteColorText(VerticalBar, TreeConsoleFormatter.GetTreeColor(i));
        }
    }

    private static int GetConsoleWidth()
    {
        try { return Console.WindowWidth; }
        catch (IOException) { return 0; }
        catch (PlatformNotSupportedException) { return 0; }
    }
}