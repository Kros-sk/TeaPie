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

    /// <summary>
    /// Convert JSON string to <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">Type which JSON string will be deserialized to.</typeparam>
    /// <param name="jsonText">String in JSON structure, object should be extracted from.</param>
    /// <returns><paramref name="jsonText"/> in a <typeparamref name="TResult"/> form.</returns>
    public static TResult? To<TResult>(this string jsonText)
        => System.Text.Json.JsonSerializer.Deserialize<TResult>(jsonText);

    /// <summary>
    /// Serializes object to <see cref="string"/> in <b>JSON structure</b>.
    /// </summary>
    /// <param name="obj">Object that should be serialized to JSON structured <see cref="string"/>.</param>
    /// <returns><see cref="string"/> which represents <paramref name="obj"/> in <b>JSON structure</b>.</returns>
    public static string ToJsonString(this object obj)
        => System.Text.Json.JsonSerializer.Serialize(obj);
}
