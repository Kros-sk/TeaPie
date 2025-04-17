using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace TeaPie.Json;

public static class JsonExtensions
{
    private static readonly JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = true };

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
        => System.Text.Json.JsonSerializer.Deserialize<TResult>(jsonText, _serializerOptions);

    /// <summary>
    /// Serializes object to <see cref="string"/> in <b>JSON structure</b>.
    /// </summary>
    /// <param name="obj">Object that should be serialized to JSON structured <see cref="string"/>.</param>
    /// <returns><see cref="string"/> which represents <paramref name="obj"/> in <b>JSON structure</b>.</returns>
    public static string ToJsonString(this object obj)
        => System.Text.Json.JsonSerializer.Serialize(obj);

    /// <summary>
    /// Merges two JSON objects represented as strings.
    /// The <paramref name="otherJson"/> object will overwrite existing properties in <paramref name="baseJson"/>
    /// or add new ones.
    /// </summary>
    /// <param name="baseJson">The original JSON string to be merged into.</param>
    /// <param name="otherJson">The JSON string containing properties to merge into <paramref name="baseJson"/>.</param>
    /// <returns>A new JSON string representing the merged result of <paramref name="baseJson"/> and
    /// <paramref name="otherJson"/>.</returns>
    public static string CombineWith(this string baseJson, string otherJson)
        => JsonHelper.Merge(baseJson, otherJson);

    /// <summary>
    /// Adds a new property with the specified name and value to the given JSON string.
    /// </summary>
    /// <typeparam name="TPropertyType">The type of the property value to add. If it is string in JSON format, it is added
    /// as another JSON node.</typeparam>
    /// <param name="jsonText">The original JSON string.</param>
    /// <param name="propertyName">The name of the property to add.</param>
    /// <param name="propertyValue">The value of the property to add.</param>
    /// <returns>A new JSON string with the added property.</returns>
    public static string AddJsonProperty<TPropertyType>(this string jsonText, string propertyName, TPropertyType propertyValue)
        => JsonHelper.AddProperty(jsonText, propertyName, propertyValue);

    /// <summary>
    /// Removes a property with the specified name from the given JSON string.
    /// </summary>
    /// <param name="jsonText">The original JSON string.</param>
    /// <param name="propertyName">The name of the property to remove.</param>
    /// <returns>A new JSON string without the specified property.</returns>
    public static string RemoveJsonProperty(this string jsonText, string propertyName)
        => JsonHelper.RemoveProperty(jsonText, propertyName);
}
