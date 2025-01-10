using Newtonsoft.Json.Linq;

namespace TeaPie;

public static class StringExtensions
{
    internal static string TrimSuffix(this string text, string suffix)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (suffix != null && text.EndsWith(suffix))
        {
            return text[..^suffix.Length];
        }

        return text;
    }

    public static JObject ToJson(this string text)
        => JObject.Parse(text);

    public static JObject ToJson(this Task<string> text)
        => JObject.Parse(text.Result);
}
