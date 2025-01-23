using FluentAssertions;
using NuGet.Protocol;
using TeaPie.Json;

namespace TeaPie.Tests.Json;

public class JsonExtensionsShould
{
    private const string JsonString = "{\"stringKey\":\"stringValue\",\"numberKey\":123,\"booleanKey\":true,\"arrayKey\":[\"value1\",2,false],\"objectKey\":{\"nestedStringKey\":\"nestedValue\",\"nestedNumberKey\":456}}";

    [Fact]
    public void ConvertJsonStringToJsonObjectCorrectly()
    {
        var json = JsonString.ToJson();
        json["stringKey"]?.ToString().Should().BeEquivalentTo("stringValue");
        ((long?)json["numberKey"]).Should().Be(123);
        ((bool?)json["booleanKey"]).Should().BeTrue();
        ((IEnumerable<object?>?)json["arrayKey"])?.Count().Should().Be(3);

        var obj = json["objectKey"];
        obj.Should().NotBeNull();
        obj?["nestedStringKey"]?.ToString().Should().BeEquivalentTo("nestedValue");
        ((long?)obj?["nestedNumberKey"]).Should().Be(456);
    }

    [Fact]
    public void ConvertJsonStringToCaseInsensitiveExpandoObjectCorrectly()
    {
        dynamic json = JsonString.ToExpando();

        Assert.Equal(json.stringKey, "stringValue");
        Assert.Equal(json.numberKey, 123);
        Assert.True(json.BooleanKey);
        Assert.Equal(json.arrayKey.Count, 3);

        Assert.NotNull(json.ObjectKey);
        Assert.Equal(json.objectKey.NestedStringKey, "nestedValue");
        Assert.Equal(json.objectKey.nestedNumberKey, 456);
    }

    [Fact]
    public void DeserializeAndSerializeCorrectly()
    {
        var obj = JsonString.To<object>();
        var json = obj?.ToJsonString();

        Assert.NotNull(obj);
        Assert.NotNull(json);
        Assert.Equal(JsonString, json);
    }
}
