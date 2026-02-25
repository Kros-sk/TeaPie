using Serilog.Events;

namespace TeaPie.Logging;

internal static class TreeConsoleWriter
{
    internal const string VerticalBar = "│  ";
    private const string StartCorner = "┌──";
    private const string EndCorner = "└──";

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