using Serilog.Events;

namespace TeaPie.Logging;

internal static class TreeConsoleWriter
{
    internal const string VerticalBar = "│  ";
    private const string StartCorner = "┌──";
    private const string EndCorner = "└──";
    internal const string TimestampFormat = "HH:mm:ss";
    internal const string DefaultOutputTemplate = $"[{{Timestamp:{TimestampFormat}}} {{Level:u3}}] {{Message:lj}}{{NewLine}}{{Exception}}";

    private const int ExpectedLevelLength = 3;
    private const int ExpectedTimestampLength = 8;
    private const int MinHeaderLengthForLevel = ExpectedLevelLength + 1;
    private const int BracketStartIndex = 0;
    private const int TimestampStartIndex = BracketStartIndex + 1;
    private const int LevelStartIndex = TimestampStartIndex + ExpectedTimestampLength + 1;
    private const int BracketEndIndex = LevelStartIndex + ExpectedLevelLength;
    private const int ExpectedMinHeaderLength = BracketEndIndex + 1;
    private static readonly ConsoleColor[] _treeColors = [
        ConsoleColor.Gray,
        ConsoleColor.DarkGray,
        ConsoleColor.Cyan,
        ConsoleColor.DarkCyan,
        ConsoleColor.DarkGray,
        ConsoleColor.DarkGray
    ];

    internal static void WriteOpening(int depth, DateTimeOffset timestamp, string levelShort)
        => WriteScopeBracket(StartCorner, depth, timestamp, levelShort);

    internal static void WriteClosing(int depth, DateTimeOffset timestamp, string levelShort)
        => WriteScopeBracket(EndCorner, depth, timestamp, levelShort);

    private static void WriteScopeBracket(string corner, int depth, DateTimeOffset timestamp, string levelShort)
    {
        var prefix = BuildPrefix(depth - 1);
        var header = BuildHeader(timestamp, levelShort) + " ";

        WriteIndent(header, prefix, levelShort);
        WriteColorText(corner, GetTreeColor(depth - 1));
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

        WriteWrappedLine(header, prefix, firstLine[headerWidth..], levelShort);

        var emptyHeader = new string(' ', headerWidth);
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
            Console.Out.WriteLine(message);
            return;
        }

        var chunkLength = Math.Max(1, width - indentLength);
        var remaining = message;
        var emptyHeader = new string(' ', header.Length);

        while (remaining.Length > 0)
        {
            WriteIndent(printHeader, prefix, printLevel);

            var currentChunkSize = Math.Min(remaining.Length, chunkLength);
            Console.Out.WriteLine(remaining[..currentChunkSize]);
            remaining = remaining[currentChunkSize..];

            printHeader = emptyHeader;
            printLevel = null;
        }
    }

    private static void WriteIndent(string header, string prefix, string? levelShort)
    {
        WriteColorizedHeader(header, levelShort);

        var depth = string.IsNullOrEmpty(prefix) ? 0 : prefix.Length / VerticalBar.Length;
        for (var i = 0; i < depth; i++)
        {
            WriteColorText(VerticalBar, GetTreeColor(i));
        }
    }

    private static void WriteColorizedHeader(string header, string? levelShort)
    {
        if (string.IsNullOrWhiteSpace(header))
        {
            Console.Out.Write(header);
            return;
        }

        if (header.Length >= ExpectedMinHeaderLength && header[BracketStartIndex] == '[')
        {
            WriteColorText("[", ConsoleColor.DarkGray);
            WriteColorText(header.Substring(TimestampStartIndex, ExpectedTimestampLength), ConsoleColor.Gray);
            Console.Out.Write(" ");
            WriteColorText(header.Substring(LevelStartIndex, ExpectedLevelLength), GetLevelColor(levelShort ?? "UNK"));
            WriteColorText(header.Substring(BracketEndIndex), ConsoleColor.DarkGray);
        }
        else
        {
            Console.Out.Write(header);
        }
    }

    private static void WriteColorText(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Out.Write(text);
        Console.ResetColor();
    }

    private static ConsoleColor GetTreeColor(int index)
        => _treeColors[Math.Max(0, index) % _treeColors.Length];

    private static ConsoleColor GetLevelColor(string levelShort) => levelShort switch
    {
        "FTL" => ConsoleColor.Magenta,
        "ERR" => ConsoleColor.Red,
        "WRN" => ConsoleColor.Yellow,
        "INF" => ConsoleColor.Green,
        "DBG" => ConsoleColor.Cyan,
        "VRB" => ConsoleColor.DarkGray,
        _ => ConsoleColor.White
    };

    private static int GetConsoleWidth()
    {
        try { return Console.WindowWidth; }
        catch (IOException) { return 0; }
        catch (PlatformNotSupportedException) { return 0; }
    }

    internal static string LevelToShort(LogEventLevel level) => level switch
    {
        LogEventLevel.Verbose => "VRB",
        LogEventLevel.Debug => "DBG",
        LogEventLevel.Information => "INF",
        LogEventLevel.Warning => "WRN",
        LogEventLevel.Error => "ERR",
        LogEventLevel.Fatal => "FTL",
        _ => "UNK",
    };
}