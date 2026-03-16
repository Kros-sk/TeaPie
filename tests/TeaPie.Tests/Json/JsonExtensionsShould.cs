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

        Assert.Equal("stringValue", json.stringKey);
        Assert.Equal(123, json.numberKey);
        Assert.True(json.BooleanKey);
        Assert.Equal(3, json.arrayKey.Count);

        Assert.NotNull(json.ObjectKey);
        Assert.Equal("nestedValue", json.objectKey.NestedStringKey);
        Assert.Equal(456, json.objectKey.nestedNumberKey);
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
