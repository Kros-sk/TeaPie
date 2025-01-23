namespace TeaPie;

internal static class StringExtensions
{
    public static string TrimSuffix(this string text, string suffix)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (suffix != null && text.EndsWith(suffix))
        {
            return text[..^suffix.Length];
        }

        return text;
    }
}
