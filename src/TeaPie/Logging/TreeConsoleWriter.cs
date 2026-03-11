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
        => WriteLine(StartCorner, depth, timestamp, levelShort);

    internal static void WriteClosing(int depth, DateTimeOffset timestamp, string levelShort)
        => WriteLine(EndCorner, depth, timestamp, levelShort);

    private static void WriteLine(string corner, int depth, DateTimeOffset timestamp, string levelShort)
    {
        var prefix = BuildPrefix(depth - 1);
        var header = BuildHeader(timestamp, levelShort);
        Console.Out.WriteLine(header + " " + prefix + corner);
    }

    internal static string BuildPrefix(int repeat)
    {
        if (repeat <= 0)
        {
            return string.Empty;
        }

        return string.Concat(Enumerable.Repeat(VerticalBar, repeat));
    }

    private static string BuildHeader(DateTimeOffset timestamp, string levelShort)
        => $"[{timestamp.ToString(TimestampFormat)} {levelShort}]";

    internal static void WriteWithPrefix(string rendered, string prefix)
    {
        var lines = rendered.Split('\n');
        var headerPadding = string.Empty;
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].TrimEnd('\r');

            if (i == lines.Length - 1 && string.IsNullOrEmpty(line))
            {
                break;
            }

            if (i == 0)
            {
                var headerEnd = line.IndexOf("] ");
                if (headerEnd >= 0)
                {
                    var headerWidth = headerEnd + 2;
                    headerPadding = new string(' ', headerWidth);
                    var fullLine = line[..headerWidth] + prefix + line[headerWidth..];
                    WriteWrapped(fullLine, headerPadding + prefix);
                }
                else
                {
                    WriteWrapped(prefix + line, headerPadding + prefix);
                }
            }
            else if (!string.IsNullOrWhiteSpace(line))
            {
                WriteWrapped(headerPadding + prefix + line, headerPadding + prefix);
            }
            else
            {
                Console.Out.WriteLine(line);
            }
        }
    }

    private static void WriteWrapped(string line, string headerPadding)
    {
        int width;
        try
        {
            width = Console.WindowWidth;
        }
        catch (IOException) { width = 0; }
        catch (PlatformNotSupportedException) { width = 0; }

        if (width <= 0 || line.Length <= width)
        {
            Console.Out.WriteLine(line);
            return;
        }

        Console.Out.Write(line[..width]);

        var remaining = line[width..];
        while (remaining.Length > 0)
        {
            var available = Math.Max(1, width - headerPadding.Length);
            var chunk = remaining.Length <= available ? remaining : remaining[..available];
            Console.Out.Write(headerPadding + chunk);
            remaining = remaining[chunk.Length..];
        }

        Console.Out.WriteLine();
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