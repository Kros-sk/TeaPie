using Serilog.Events;

namespace TeaPie.Logging;

internal static class TreeConsoleFormatter
{
    private static readonly ConsoleColor[] _treeColors = [
        ConsoleColor.Gray,
        ConsoleColor.DarkGray,
        ConsoleColor.Cyan,
        ConsoleColor.Yellow,
        ConsoleColor.DarkGray,
        ConsoleColor.DarkGray
    ];

    private static readonly Dictionary<Func<string, bool>, string> _symbolMappings = new()
    {
        { msg => msg.StartsWith("Test Passed:") || msg.StartsWith("Test passed during retry"), "✔ " },
        { msg => msg.StartsWith("Test '") && msg.Contains("failed"), "✘ " },
        { msg => msg.StartsWith("Skipping test:") || (msg.StartsWith("Test '") && msg.Contains("already executed")), "⏭ " },
        { msg => msg.StartsWith("Reason:"), "  " }
    };

    internal static string PrependSymbol(string message)
    {
        foreach (var (predicate, symbol) in _symbolMappings)
        {
            if (predicate(message))
            {
                return symbol + message;
            }
        }

        return message;
    }

    internal static void WriteMessageWithSymbolColor(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            Console.Out.WriteLine();
            return;
        }

        switch (message[0])
        {
            case '✔':
                WriteColorText("✔", ConsoleColor.Green);
                Console.Out.WriteLine(message[1..]);
                break;
            case '✘':
                WriteColorText("✘", ConsoleColor.Red);
                Console.Out.WriteLine(message[1..]);
                break;
            case '⏭':
                WriteColorText("⏭", ConsoleColor.Blue);
                Console.Out.WriteLine(message[1..]);
                break;
            default:
                Console.Out.WriteLine(message);
                break;
        }
    }

    internal static void WriteColorizedHeader(string header, string? levelShort)
    {
        if (string.IsNullOrWhiteSpace(header))
        {
            Console.Out.Write(header);
            return;
        }

        if (header.Length >= TreeConsoleWriter.ExpectedMinHeaderLength && header[TreeConsoleWriter.BracketStartIndex] == '[')
        {
            WriteColorText("[", ConsoleColor.DarkGray);
            WriteColorText(header.Substring(TreeConsoleWriter.TimestampStartIndex, TreeConsoleWriter.ExpectedTimestampLength), ConsoleColor.Gray);
            Console.Out.Write(" ");
            WriteColorText(header.Substring(TreeConsoleWriter.LevelStartIndex, TreeConsoleWriter.ExpectedLevelLength), GetLevelColor(levelShort ?? "UNK"));
            WriteColorText(header.Substring(TreeConsoleWriter.BracketEndIndex), ConsoleColor.DarkGray);
        }
        else
        {
            Console.Out.Write(header);
        }
    }

    internal static void WriteColorText(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Out.Write(text);
        Console.ResetColor();
    }

    internal static ConsoleColor GetTreeColor(int index)
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
