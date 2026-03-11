using Serilog.Events;

namespace TeaPie.Logging;

internal static class TreeConsoleWriter
{
    internal const string VerticalBar = "│  ";
    private const string StartCorner = "┌──";
    private const string EndCorner = "└──";
    internal const string TimestampFormat = "HH:mm:ss";
    internal const string DefaultOutputTemplate = $"[{{Timestamp:{TimestampFormat}}} {{Level:u3}}] {{Message:lj}}{{NewLine}}{{Exception}}";

    internal static void WriteOpening(int depth, DateTimeOffset timestamp, string levelShort)
        => WriteScopeBracket(StartCorner, depth, timestamp, levelShort);

    internal static void WriteClosing(int depth, DateTimeOffset timestamp, string levelShort)
        => WriteScopeBracket(EndCorner, depth, timestamp, levelShort);

    private static void WriteScopeBracket(string corner, int depth, DateTimeOffset timestamp, string levelShort)
    {
        var prefix = BuildPrefix(depth - 1);
        var header = BuildHeader(timestamp, levelShort);
        Console.Out.WriteLine(header + " " + prefix + corner);
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
        var wrapIndent = new string(' ', headerWidth) + prefix;

        WriteWrappedLine(firstLine[..headerWidth] + prefix + firstLine[headerWidth..], wrapIndent);

        foreach (var rawLine in lines.Skip(1))
        {
            WriteWrappedLine(wrapIndent + rawLine.TrimEnd('\r'), wrapIndent);
        }
    }

    private static void WriteWrappedLine(string line, string wrapIndent)
    {
        var width = GetConsoleWidth();

        if (width <= 0 || line.Length <= width)
        {
            Console.Out.WriteLine(line);
            return;
        }

        Console.Out.Write(line[..width]);

        var remaining = line[width..];
        while (remaining.Length > 0)
        {
            var available = Math.Max(1, width - wrapIndent.Length);
            var chunk = remaining.Length <= available ? remaining : remaining[..available];
            Console.Out.Write(wrapIndent + chunk);
            remaining = remaining[chunk.Length..];
        }

        Console.Out.WriteLine();
    }

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