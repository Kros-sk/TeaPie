using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TeaPie.Json;

public static class JsonExtensions
{
    /// <summary>
    /// Parses given text to <see cref="JObject"/> form.
    /// </summary>
    /// <param name="text">Text to be parsed into <see cref="JObject"/>.</param>
    /// <returns><see cref="JObject"/> representation of JSON within <paramref name="text"/>.</returns>
    public static JObject ToJson(this string text)
        => JObject.Parse(text);

    /// <summary>
    /// Parses given text (in JSON structure) to <b>case-insensitive</b> expando object
    /// (<see cref="CaseInsensitiveExpandoObject"/>).
    /// </summary>
    /// <param name="jsonText">Text to be parsed into <b>case-insensitive</b> expando object
    /// (<see cref="CaseInsensitiveExpandoObject"/>).</param>
    /// <returns><see cref="CaseInsensitiveExpandoObject"/> representation of JSON stored in <paramref name="jsonText"/>.
    /// </returns>
    public static CaseInsensitiveExpandoObject ToExpando(this string jsonText)
        => new(JsonConvert.DeserializeObject<Dictionary<string, object?>>(jsonText) ?? []);
}
