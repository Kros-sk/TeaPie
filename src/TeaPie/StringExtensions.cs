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

    /// <summary>
    /// Parses given text to json object form.
    /// </summary>
    /// <param name="text">Text to be parsed into json object.</param>
    /// <returns><see cref="JObject"/> representation of json stored in <paramref name="text"/>.</returns>
    public static JObject ToJson(this string text)
        => JObject.Parse(text);

    /// <summary>
    /// Parses given text (in form of <see cref="Task{T}"/>) to json object form.
    /// </summary>
    /// <param name="text">Task which product is text that will be parsed into json object.</param>
    /// <returns><see cref="JObject"/> representation of json stored in <paramref name="text"/>.</returns>
    public static JObject ToJson(this Task<string> text)
        => JObject.Parse(text.Result);
}
