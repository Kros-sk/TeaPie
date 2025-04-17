using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TeaPie.Json;

internal static class JsonHelper
{
    private const double Epsilon = 1e-8;
    private static readonly JsonSerializerOptions _jsonSerializeOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static bool JsonContains(
        string containerJson,
        string containedJson,
        [NotNullWhen(false)] out (string expected, string found)? error,
        params string[] ignoreProperties)
    {
        using var containerDoc = JsonDocument.Parse(containerJson);
        using var containedDoc = JsonDocument.Parse(containedJson);

        return Contains(
            containerDoc.RootElement,
            containedDoc.RootElement,
            out error,
            new HashSet<string>(ignoreProperties, StringComparer.OrdinalIgnoreCase));
    }

    public static string Merge(string baseJson, string otherJson)
    {
        var node1 = JsonNode.Parse(baseJson)?.AsObject() ?? [];
        var node2 = JsonNode.Parse(otherJson)?.AsObject() ?? [];

        var merged = DeepMerge(node1, node2);
        return merged.ToJsonString(_jsonSerializeOptions);
    }

    public static string AddProperty<T>(string jsonText, string propertyName, T propertyValue)
    {
        var jsonObject = ParseAndEnsureJsonObject(jsonText);

        jsonObject[propertyName] = propertyValue switch
        {
            null => null,
            JsonNode node => node,
            string strValue when IsValidJson(strValue)
                => JsonSerializer.Deserialize<JsonNode>(strValue, _jsonSerializeOptions),
            _ => JsonNode.Parse(JsonSerializer.Serialize(propertyValue, _jsonSerializeOptions))
        };

        return JsonSerializer.Serialize(jsonObject, _jsonSerializeOptions);
    }

    public static string RemoveProperty(string jsonText, string propertyName)
    {
        if (string.IsNullOrEmpty(jsonText) || string.IsNullOrEmpty(propertyName))
        {
            return jsonText;
        }

        try
        {
            var jsonObject = ParseAndEnsureJsonObject(jsonText);

            jsonObject.Remove(propertyName);

            return JsonSerializer.Serialize(jsonObject, _jsonSerializeOptions);
        }
        catch (JsonException)
        {
            return jsonText;
        }
    }

    private static bool IsValidJson(string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return false;
        }

        try
        {
            JsonNode.Parse(jsonString);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool Contains(
        JsonElement container, JsonElement contained, out (string expected, string found)? error, HashSet<string> ignoreProperties)
    {
        if (contained.ValueKind != JsonValueKind.Object || container.ValueKind != JsonValueKind.Object)
        {
            error = ("JSON object", "invalid JSON object was given");
            return false;
        }

        foreach (var prop in contained.EnumerateObject())
        {
            if (ignoreProperties.Contains(prop.Name))
            {
                continue;
            }

            if (!container.TryGetProperty(prop.Name, out var containerProp))
            {
                error = ($"to contain property '{prop.Name}'", $"no property '{prop.Name}' was found");
                return false;
            }

            if (!JsonElementEquals(containerProp, prop.Value, prop.Name, out error, ignoreProperties))
            {
                return false;
            }
        }

        error = null;
        return true;
    }

    private static bool JsonElementEquals(
        JsonElement elementA,
        JsonElement elementB,
        string propertyName,
        out (string expected, string found)? error,
        HashSet<string> ignoreProperties)
    {
        if (elementA.ValueKind != elementB.ValueKind)
        {
            error = (
                $"to have property '{propertyName}' of '{elementA.ValueKind}' type",
                $"'{propertyName}' with '{elementB.ValueKind}' found");
            return false;
        }

        var equals = elementA.ValueKind switch
        {
            JsonValueKind.String => elementA.GetString() == elementB.GetString(),
            JsonValueKind.Number => Math.Abs(elementA.GetDouble() - elementB.GetDouble()) <= Epsilon,
            JsonValueKind.True => elementB.ValueKind == JsonValueKind.True,
            JsonValueKind.False => elementB.ValueKind == JsonValueKind.False,
            JsonValueKind.Null => elementB.ValueKind == JsonValueKind.Null,
            JsonValueKind.Object => Contains(elementA, elementB, out error, ignoreProperties),
            JsonValueKind.Array => CompareArrays(elementA, elementB, propertyName, out error, ignoreProperties),
            _ => false,
        };

        error = ($"to have property '{propertyName}' with '{elementA}' value",
            $"found property '{propertyName}' with '{elementB}' value");
        return equals;
    }

    private static bool CompareArrays(
        JsonElement arrayA,
        JsonElement arrayB,
        string propertyName,
        out (string expected, string found)? error,
        HashSet<string> ignoreProperties)
    {
        if (arrayA.GetArrayLength() != arrayB.GetArrayLength())
        {
            error = ($"to have an array property '{propertyName}' with the length of {arrayA.GetArrayLength()}",
                $"found an array property '{propertyName}' with '{arrayB.GetArrayLength()}' value");
            return false;
        }

        for (var i = 0; i < arrayA.GetArrayLength(); i++)
        {
            if (!JsonElementEquals(arrayA[i], arrayB[i], propertyName, out error, ignoreProperties))
            {
                return false;
            }
        }

        error = null;
        return true;
    }

    private static JsonObject DeepMerge(JsonObject target, JsonObject source)
    {
        foreach (var kvp in source)
        {
            if (kvp.Value is JsonObject sourceObj &&
                target[kvp.Key] is JsonObject targetObj)
            {
                target[kvp.Key] = DeepMerge(targetObj, sourceObj);
            }
            else
            {
                target[kvp.Key] = kvp.Value?.DeepClone();
            }
        }

        return target;
    }

    private static JsonObject ParseAndEnsureJsonObject(string jsonText)
    {
        if (string.IsNullOrWhiteSpace(jsonText))
        {
            return [];
        }

        try
        {
            var rootNode = JsonNode.Parse(jsonText);

            if (rootNode is JsonObject obj)
            {
                return obj;
            }
            else
            {
                return new JsonObject
                {
                    ["value"] = rootNode
                };
            }
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
