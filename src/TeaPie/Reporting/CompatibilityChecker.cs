using System.Text;

namespace TeaPie.Reporting;

internal static class CompatibilityChecker
{
    public static readonly bool SupportsEmoji = SupportsEmojis();

    private static bool SupportsEmojis()
    {
        if (!Console.OutputEncoding.Equals(Encoding.UTF8))
        {
            return false;
        }

        if (OperatingSystem.IsWindows() && Environment.UserInteractive && Console.Title == "Command Prompt")
        {
            return false;
        }

        return TestEmojiRendering();
    }

    private static bool TestEmojiRendering()
    {
        const string emoji = "😀";

        var originalOutput = Console.Out;

        try
        {
            using (var testOutput = new StringWriter())
            {
                Console.SetOut(testOutput);

                Console.Write(emoji);

                var result = testOutput.ToString();

                return result.Contains(emoji);
            }
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }
}
