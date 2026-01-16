using Serilog.Events;

namespace TeaPie.Logging;

internal static class TreeConsoleWriter
{
    private const string VerticalBar = "│  ";
    private const string StartCorner = "┌──";
    private const string EndCorner = "└──";

    internal static void WriteOpening(int depth, DateTimeOffset timestamp, string levelShort)
    {
        var prefix = BuildPrefix(depth - 1);
        var header = BuildHeader(timestamp, levelShort);
        Console.Out.WriteLine(header + " " + prefix + StartCorner);
    }

    internal static void WriteOpening(int depth) => WriteOpening(depth, DateTimeOffset.Now, LevelToShort(LogEventLevel.Information));

    internal static void WriteClosing(int depth, DateTimeOffset timestamp, string levelShort)
    {
        var prefix = BuildPrefix(depth - 1);
        var header = BuildHeader(timestamp, levelShort);
        Console.Out.WriteLine(header + " " + prefix + EndCorner);
    }

    internal static void WriteClosing(int depth) => WriteClosing(depth, DateTimeOffset.Now, LevelToShort(LogEventLevel.Information));

    private static string BuildPrefix(int repeat)
    {
        if (repeat <= 0)
        {
            return string.Empty;
        }

        return string.Concat(Enumerable.Repeat(VerticalBar, repeat));
    }

    private static string BuildHeader(DateTimeOffset timestamp, string levelShort)
    {
        return $"[{timestamp:HH:mm:ss} {levelShort}]";
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