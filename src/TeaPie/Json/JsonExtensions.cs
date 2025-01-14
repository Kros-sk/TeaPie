using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TeaPie.Json;

public static class JsonExtensions
{
    /// <summary>
    /// Parses given text to json object form.
    /// </summary>
    /// <param name="text">Text to be parsed into json object.</param>
    /// <returns><see cref="JObject"/> representation of json stored in <paramref name="text"/>.</returns>
    public static JObject ToJson(this string text)
        => JObject.Parse(text);

    /// <summary>
    /// Parses given json text to <b>case-insensitive</b> expando object (<see cref="CaseInsensitiveExpandoObject"/>).
    /// </summary>
    /// <param name="jsonText">Text in json structure to be parsed into json object.</param>
    /// <returns><see cref="CaseInsensitiveExpandoObject"/> representation of json stored in <paramref name="jsonText"/>.
    /// </returns>
    public static CaseInsensitiveExpandoObject ToJsonExpando(this string jsonText)
        => new(JsonConvert.DeserializeObject<Dictionary<string, object?>>(jsonText) ?? []);
}
