namespace TeaPie.Helpers;

internal static class StringHelper
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
