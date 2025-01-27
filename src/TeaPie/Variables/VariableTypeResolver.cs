using System.Text.Json;

namespace TeaPie.Variables;

internal static class VariableTypeResolver
{
    public static object? Resolve(object variableValue)
    {
        if (variableValue is null)
        {
            return null;
        }

        if (variableValue is JsonElement element)
        {
            return ResolveJsonElement(element);
        }

        return variableValue;
    }

    private static object? ResolveJsonElement(JsonElement element)
        => element.ValueKind switch
        {
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
            list.Add(Resolve(item));
        }

        return list;
    }
}
