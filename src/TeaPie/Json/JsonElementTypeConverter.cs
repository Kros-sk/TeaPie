using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeaPie.Json;

internal class JsonElementTypeConverter : JsonConverter<Dictionary<string, object?>>
{
    public override Dictionary<string, object?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dictionary = new Dictionary<string, object?>();

        using (var document = JsonDocument.ParseValue(ref reader))
        {
            foreach (var element in document.RootElement.EnumerateObject())
            {
                dictionary[element.Name] = Convert(element.Value);
            }
        }

        return dictionary;
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, object?> value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, options);

    public static object? Convert(object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is JsonElement element)
        {
            return ResolveJsonElement(element);
        }

        return value;
    }

    private static object? ResolveJsonElement(JsonElement element)
        => element.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.String => ResolveString(element),
            JsonValueKind.True or JsonValueKind.False => element.GetBoolean(),
            JsonValueKind.Number => ResolveNumber(element),
            JsonValueKind.Array => ResolveArray(element),
            _ => ResolveString(element),
        };

    private static object? ResolveString(JsonElement element)
        => element.TryGetGuid(out var guidValue) ? guidValue :
            element.TryGetDateTimeOffset(out var dateTimeOffsetValue) ? dateTimeOffsetValue : element.GetString();

    private static object? ResolveNumber(JsonElement element)
        => element.TryGetInt32(out var integerValue) ? integerValue :
            element.TryGetInt64(out var longValue) ? longValue :
            element.TryGetDecimal(out var decimalValue) ? decimalValue : element;

    private static List<object?> ResolveArray(JsonElement element)
    {
        var list = new List<object?>();
        foreach (var item in element.EnumerateArray())
        {
            list.Add(Convert(item));
        }

        return list;
    }
}
