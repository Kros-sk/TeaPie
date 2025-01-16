namespace TeaPie.Reporting;

internal static class CompatibilityChecker
{
    public static bool SupportsEmojis()
    {
        var isUtf8 = Console.OutputEncoding.WebName.Contains("utf", StringComparison.OrdinalIgnoreCase);
        if (!isUtf8)
        {
            return false;
        }

        const string emoji = "😀";
        var cursorLeftBefore = Console.CursorLeft;

        Console.Write(emoji);
        Console.SetCursorPosition(cursorLeftBefore, Console.CursorTop);
        Console.Write(" ");
        Console.SetCursorPosition(cursorLeftBefore, Console.CursorTop);

        return Console.CursorLeft - cursorLeftBefore == 1;
    }
}
