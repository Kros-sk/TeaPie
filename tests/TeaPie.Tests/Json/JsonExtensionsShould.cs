using FluentAssertions;
using NuGet.Protocol;
using TeaPie.Json;

namespace TeaPie.Tests.Json;

public class JsonExtensionsShould
{
    public const string JsonString =
"""
{
    "stringKey": "stringValue",
    "numberKey": 123,
    "booleanKey": true,
    "arrayKey": [
        "value1",
        2,
        false
    ],
    "objectKey": {
        "nestedStringKey": "nestedValue",
        "nestedNumberKey": 456
    }
}
""";

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
        //dynamic json = JsonString.ToJsonExpando();

        //StringShould(json.stringKey).BeEquivalentTo("stringValue");
        //LongShould(json.numberKey).Be(123);
        //BoolShould(json.BooleanKey).BeTrue();
        //CollectionShould(json.arrayKey).HaveCount(3);

        //ObjectShould(json.ObjectKey).NotBeNull();
        //StringShould(json.objectKey.NestedStringKey).BeEquivalentTo("nestedValue");
        //LongShould(json.objectKey.nestedNumberKey).Be(456);
    }
}
