using System.Text.Json;
using TeaPie.Json;

namespace TeaPie.Tests.Json;

public class JsonElementTypeConverterShould
{
    public const string Json = """
{
    "booleanProperty": true,
    "integerProperty": 42,
    "longProperty": 9223372036854775807,
    "decimalProperty": 12345.6789,
    "stringProperty": "Hello, World!",
    "guidProperty": "d2713b57-3494-4d0a-8e3b-2e587f3e8b3e",
    "dateTimeProperty": "2025-01-27T12:34:56",
    "dateTimeOffsetProperty": "2025-01-27T12:34:56+01:00",
    "arrayProperty": [1, 2, 3, 4, 5]
}
""";

    [Fact]
    public void ResolveBooleanFromJsonElementCorrectly()
    {
        var deserialized = JsonSerializer.Deserialize<Dictionary<string, object>>(Json);
        var resolved = JsonElementTypeConverter.Convert(deserialized!["booleanProperty"]);

        Assert.Equal(resolved, true);
    }

    [Fact]
    public void ResolveIntegerFromJsonElementCorrectly()
    {
        var deserialized = JsonSerializer.Deserialize<Dictionary<string, object>>(Json);
        var resolved = JsonElementTypeConverter.Convert(deserialized!["integerProperty"]);

        Assert.Equal(resolved, 42);
    }

    [Fact]
    public void ResolveLongFromJsonElementCorrectly()
    {
        var deserialized = JsonSerializer.Deserialize<Dictionary<string, object>>(Json);
        var resolved = JsonElementTypeConverter.Convert(deserialized!["longProperty"]);

        Assert.Equal(resolved, 9223372036854775807L);
    }

    [Fact]
    public void ResolveDecimalFromJsonElementCorrectly()
    {
        var deserialized = JsonSerializer.Deserialize<Dictionary<string, object>>(Json);
        var resolved = JsonElementTypeConverter.Convert(deserialized!["decimalProperty"]);

        Assert.Equal(resolved, 12345.6789m);
    }

    [Fact]
    public void ResolveStringFromJsonElementCorrectly()
    {
        var deserialized = JsonSerializer.Deserialize<Dictionary<string, object>>(Json);
        var resolved = JsonElementTypeConverter.Convert(deserialized!["stringProperty"]);

        Assert.Equal(resolved, "Hello, World!");
    }

    [Fact]
    public void ResolveGuidFromJsonElementCorrectly()
    {
        var deserialized = JsonSerializer.Deserialize<Dictionary<string, object>>(Json);
        var resolved = JsonElementTypeConverter.Convert(deserialized!["guidProperty"]);

        Assert.Equal(resolved, Guid.Parse("d2713b57-3494-4d0a-8e3b-2e587f3e8b3e"));
    }

    [Fact]
    public void ResolveDateTimeOffsetFromJsonElementCorrectly()
    {
        var deserialized = JsonSerializer.Deserialize<Dictionary<string, object>>(Json);
        var resolved = JsonElementTypeConverter.Convert(deserialized!["dateTimeOffsetProperty"]);

        Assert.Equal(resolved, DateTimeOffset.Parse("2025-01-27T12:34:56+01:00"));
    }

    [Fact]
    public void ResolveArrayFromJsonElementCorrectly()
    {
        var deserialized = JsonSerializer.Deserialize<Dictionary<string, object>>(Json);
        var resolved = JsonElementTypeConverter.Convert(deserialized!["arrayProperty"]);

        Assert.Equal(resolved, new List<object> { 1, 2, 3, 4, 5 });
    }
}
