using TeaPie.Json;
using static Xunit.Assert;
using static TeaPie.Testing.XunitAssertExtensions;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace TeaPie.Tests.Json;

public class JsonHelperShould
{
    private const string EmptyJson = "{}";

    [Theory]
    [MemberData(nameof(JsonContainsTestCases))]
    public void CorrectlyCheckWheterOneJsonContainsAnother(string container, string contained, bool shouldPass)
    {
        var result = JsonHelper.JsonContains(container, contained, out _);
        Equal(shouldPass, result);
    }

    [Theory]
    [MemberData(nameof(JsonContainsWithIgnoreTestCases))]
    public void ConsiderIgnoredProperties(string container, string contained, string[] ignoreProperties, bool shouldPass)
    {
        var result = JsonHelper.JsonContains(container, contained, out _, ignoreProperties);
        Equal(shouldPass, result);
    }

    [Theory]
    [MemberData(nameof(JsonMergeTestCases))]
    public void MergeTwoJsonsCorrectly(string baseJson, string otherJson, string mergedJson)
    {
        var result = JsonHelper.Merge(baseJson, otherJson);
        Equal(NormalizeJson(mergedJson), NormalizeJson(result));
        JsonContains(mergedJson, otherJson);
    }

    private static string NormalizeJson(string jsonText)
        => jsonText.Replace(" ", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);

    public static IEnumerable<object[]> JsonContainsTestCases()
    {
        yield return new object[]
        {
            @"{ ""name"": ""Alice"", ""age"": 30 }",
            @"{ ""name"": ""Alice"", ""age"": 30 }",
            true
        };

        yield return new object[]
        {
            @"{ ""name"": ""Alice"", ""age"": 30, ""city"": ""London"" }",
            @"{ ""name"": ""Alice"", ""age"": 30 }",
            true
        };

        yield return new object[]
        {
            @"{ ""name"": ""Alice"" }",
            @"{ ""name"": ""Alice"", ""age"": 30 }",
            false
        };

        yield return new object[]
        {
            @"{ ""name"": ""Alice"", ""age"": 25 }",
            @"{ ""name"": ""Alice"", ""age"": 30 }",
            false
        };

        yield return new object[]
        {
            @"{ ""user"": { ""name"": ""Alice"", ""age"": 30 }, ""active"": true }",
            @"{ ""user"": { ""name"": ""Alice"" } }",
            true
        };

        yield return new object[]
        {
            @"{ ""user"": { ""name"": ""Alice"", ""age"": 30 }, ""active"": true }",
            @"{ ""user"": { ""name"": ""Bob"" } }",
            false
        };

        yield return new object[]
        {
            @"{ ""ids"": [1, 2, 3] }",
            @"{ ""ids"": [1, 2, 3] }",
            true
        };

        yield return new object[]
        {
            @"{ ""ids"": [1, 2, 3] }",
            @"{ ""ids"": [3, 2, 1] }",
            false
        };

        yield return new object[]
        {
            @"{ ""a"": 1, ""b"": 2 }",
            @"{ }",
            true
        };

        yield return new object[]
        {
            @"{ }",
            @"{ ""a"": 1 }",
            false
        };

        yield return new object[]
        {
            @"{ ""value"": 1 }",
            @"{ ""value"": 1.0 }",
            true
        };

        yield return new object[]
        {
            @"{ ""name"": null }",
            @"{ ""name"": null }",
            true
        };

        yield return new object[]
        {
            @"{ ""name"": ""Alice"" }",
            @"{ ""name"": null }",
            false
        };

        yield return new object[]
        {
            @"{ ""pi"": 3.1415926535 }",
            @"{ ""pi"": 3.1415926536 }",
            true
        };

        yield return new object[]
        {
            @"{ ""pi"": 3.14 }",
            @"{ ""pi"": 3.14159 }",
            false
        };
    }

    public static IEnumerable<object[]> JsonContainsWithIgnoreTestCases()
    {
        yield return new object[]
        {
            @"{ ""name"": ""Alice"", ""age"": 30 }",
            @"{ ""name"": ""Alice"", ""age"": 25 }",
            new[] { "age" },
            true
        };

        yield return new object[]
        {
            @"{ ""name"": ""Alice"", ""age"": 30 }",
            @"{ ""name"": ""Alice"", ""age"": 25 }",
            Array.Empty<string>(),
            false
        };

        yield return new object[]
        {
            @"{ ""name"": ""Alice"", ""lastModified"": ""2024-01-01"" }",
            @"{ ""name"": ""Alice"" }",
            new[] { "lastModified" },
            true
        };

        yield return new object[]
        {
            @"{ ""user"": { ""name"": ""Alice"", ""age"": 30 } }",
            @"{ ""user"": { ""name"": ""Alice"", ""age"": 99 } }",
            new[] { "age" },
            true
        };

        yield return new object[]
        {
            @"{ ""user"": { ""name"": ""Alice"", ""age"": 30 } }",
            @"{ ""user"": { ""name"": ""Alice"", ""age"": 99 } }",
            Array.Empty<string>(),
            false
        };

        yield return new object[]
        {
            @"{ ""x"": 1, ""y"": 2 }",
            @"{ ""x"": 1 }",
            new[] { "nonExistentKey" },
            true
        };

        yield return new object[]
        {
            @"{ ""x"": 1, ""y"": 2 }",
            @"{ ""x"": 1, ""y"": 999 }",
            new[] { "y" },
            true
        };
    }

    public static IEnumerable<object[]> JsonMergeTestCases()
    {
        yield return new object[]
        {
            @"{ ""name"": ""Alice"", ""age"": 30 }",
            @"{ ""age"": 35, ""city"": ""London"" }",
            @"{ ""name"": ""Alice"", ""age"": 35, ""city"": ""London"" }"
        };

        yield return new object[]
        {
            @"{ ""user"": { ""name"": ""Alice"", ""age"": 30 } }",
            @"{ ""user"": { ""age"": 35, ""city"": ""London"" }, ""active"": true }",
            @"{ ""user"": { ""name"": ""Alice"", ""age"": 35, ""city"": ""London"" }, ""active"": true }"
        };

        yield return new object[]
        {
            @"{ ""name"": ""Alice"" }",
            @"{ ""age"": 30, ""city"": ""London"" }",
            @"{ ""name"": ""Alice"", ""age"": 30, ""city"": ""London"" }"
        };

        yield return new object[]
        {
            @"{ ""name"": ""Alice"", ""city"": ""New York"" }",
            @"{ ""city"": null }",
            @"{ ""name"": ""Alice"", ""city"": null }"
        };

        yield return new object[]
        {
            @"{ ""name"": ""Alice"", ""age"": 30 }",
            @"{}",
            @"{ ""name"": ""Alice"", ""age"": 30 }"
        };

        yield return new object[]
        {
            @"{ ""name"": ""Alice"" }",
            @"{ ""active"": true }",
            @"{ ""name"": ""Alice"", ""active"": true }"
        };

        yield return new object[]
        {
            @"{ ""ids"": [1, 2, 3] }",
            @"{ ""ids"": [4, 5, 6] }",
            @"{ ""ids"": [4, 5, 6] }"
        };

        yield return new object[]
        {
            @"{ ""items"": [{ ""id"": 1, ""name"": ""Item1"" }] }",
            @"{ ""items"": [{ ""id"": 2, ""name"": ""Item2"" }] }",
            @"{ ""items"": [{ ""id"": 2, ""name"": ""Item2"" }] }"
        };

        yield return new object[]
        {
            @"{ ""user"": { ""name"": ""Alice"", ""roles"": [""Admin""] } }",
            @"{ ""user"": { ""roles"": [""User"", ""Manager""] } }",
            @"{ ""user"": { ""name"": ""Alice"", ""roles"": [""User"", ""Manager""] } }"
        };
    }

    #region AddProperty

    public static IEnumerable<object?[]> EmptyJsonTestData()
    {
        yield return new object[] { "{}", "name", "John", "John" };
        yield return new object?[] { null, "name", "John", "John" };
        yield return new object[] { "   ", "name", "John", "John" };
    }

    [Theory]
    [MemberData(nameof(EmptyJsonTestData))]
    public void AddPropertyToEmptyOrNullJson(string json, string propName, string propValue, string expectedValue)
    {
        var result = JsonHelper.AddProperty(json, propName, propValue);

        var resultObj = JsonNode.Parse(result)!.AsObject();
        Equal(expectedValue, resultObj[propName]!.GetValue<string>());
    }

    public static IEnumerable<object[]> ExistingJsonTestData()
    {
        yield return new object[] { @"{""id"": 123}", "name", "John" };
        yield return new object[] { @"{""user"": {""id"": 123}}", "status", "active" };
    }

    [Theory]
    [MemberData(nameof(ExistingJsonTestData))]
    public void AddNewPropertyToExistingJson(string json, string propName, string propValue)
    {
        var result = JsonHelper.AddProperty(json, propName, propValue);

        var resultObj = JsonNode.Parse(result)!.AsObject();
        Equal(propValue, resultObj[propName]!.GetValue<string>());
        True(resultObj.Count > 1);
    }

    [Theory]
    [InlineData(@"{""name"": ""Jane""}", "name", "John")]
    [InlineData(@"{""age"": 25}", "age", 30)]
    public void OverwriteExistingProperty(string json, string propName, object propValue)
    {
        var result = JsonHelper.AddProperty(json, propName, propValue);

        var resultObj = JsonNode.Parse(result)!.AsObject();

        if (propValue is int intValue)
        {
            Equal(intValue, resultObj[propName]!.GetValue<int>());
        }
        else
        {
            Equal(propValue.ToString(), resultObj[propName]!.GetValue<string>());
        }
    }

    public static IEnumerable<object[]> PrimitiveValueTestData()
    {
        yield return new object[] { "age", 30, typeof(int) };
        yield return new object[] { "isActive", true, typeof(bool) };
        yield return new object[] { "price", 99.99, typeof(double) };
    }

    [Theory]
    [MemberData(nameof(PrimitiveValueTestData))]
    public void AddPrimitiveValuesCorrectly(string propName, object propValue, Type expectedType)
    {
        var result = JsonHelper.AddProperty(EmptyJson, propName, propValue);

        var resultObj = JsonNode.Parse(result)!.AsObject();

        if (expectedType == typeof(int))
        {
            Equal(propValue, resultObj[propName]!.GetValue<int>());
        }
        else if (expectedType == typeof(bool))
        {
            Equal(propValue, resultObj[propName]!.GetValue<bool>());
        }
        else if (expectedType == typeof(double))
        {
            Equal(propValue, resultObj[propName]!.GetValue<double>());
        }
    }

    [Theory]
    [InlineData("nullProp")]
    public void AddNullPropertyValue(string propName)
    {
        var result = JsonHelper.AddProperty<string>(EmptyJson, propName, null);

        var resultObj = JsonNode.Parse(result)!.AsObject();
        Null(resultObj[propName]);
    }

    public static IEnumerable<object[]> ComplexValueTestData()
    {
        yield return new object[] {
            "address",
            new Address { Street = "123 Main St", City = "Anytown", ZipCode = "12345" },
            new Dictionary<string, string> {
                { "Street", "123 Main St" },
                { "City", "Anytown" },
                { "ZipCode", "12345" }
            }
        };

        yield return new object[] {
            "contact",
            new Contact { Name = "John Doe", Email = "john@example.com", Phone = "555-1234" },
            new Dictionary<string, string> {
                { "Name", "John Doe" },
                { "Email", "john@example.com" },
                { "Phone", "555-1234" }
            }
        };
    }

    [Theory]
    [MemberData(nameof(ComplexValueTestData))]
    public void AddComplexObjectCorrectly(string propName, object propValue, Dictionary<string, string> expectedValues)
    {
        var result = JsonHelper.AddProperty(EmptyJson, propName, propValue);

        var resultObj = JsonNode.Parse(result)!.AsObject();
        var valueObj = resultObj[propName]!.AsObject();

        foreach (var kvp in expectedValues)
        {
            Equal(kvp.Value, valueObj[kvp.Key]!.GetValue<string>());
        }
    }

    public static IEnumerable<object[]> ArrayValueTestData()
    {
        yield return new object[] { "tags", new[] { "tag1", "tag2", "tag3" } };
        yield return new object[] { "numbers", new[] { 1, 2, 3 } };
    }

    [Theory]
    [MemberData(nameof(ArrayValueTestData))]
    public void AddArrayValueCorrectly(string propName, object arrayValue)
    {
        var result = JsonHelper.AddProperty(EmptyJson, propName, arrayValue);

        var resultObj = JsonNode.Parse(result)!.AsObject();
        var valueArray = resultObj[propName]!.AsArray();

        Equal(3, valueArray.Count);

        if (arrayValue is string[] stringArray)
        {
            for (var i = 0; i < stringArray.Length; i++)
            {
                Equal(stringArray[i], valueArray[i]!.GetValue<string>());
            }
        }
        else if (arrayValue is int[] intArray)
        {
            for (var i = 0; i < intArray.Length; i++)
            {
                Equal(intArray[i], valueArray[i]!.GetValue<int>());
            }
        }
    }

    public static IEnumerable<object[]> NonObjectJsonTestData()
    {
        yield return new object[] { "[1, 2, 3]", "name", "John", "value", JsonValueKind.Array };
        yield return new object[] { "42", "name", "John", "value", JsonValueKind.Number };
        yield return new object[] { "\"hello\"", "name", "John", "value", JsonValueKind.String };
    }

    [Theory]
    [MemberData(nameof(NonObjectJsonTestData))]
    public void WrapNonObjectJsonCorrectly(string json, string propName, string propValue, string originalValueProp, JsonValueKind expectedKind)
    {
        var result = JsonHelper.AddProperty(json, propName, propValue);

        var resultObj = JsonNode.Parse(result)!.AsObject();
        Equal(propValue, resultObj[propName]!.GetValue<string>());
        Equal(expectedKind, resultObj[originalValueProp]!.GetValueKind());
    }

    [Theory]
    [InlineData("{invalid json}")]
    [InlineData("[broken array")]
    public void CreateNewJsonObjectForInvalidJson(string json)
    {
        var result = JsonHelper.AddProperty(json, "name", "John");

        var resultObj = JsonNode.Parse(result)!.AsObject();
        Equal("John", resultObj["name"]!.GetValue<string>());
        Single(resultObj);
    }

    [Theory]
    [InlineData(@"{""id"": 123, ""name"": ""Test""}")]
    public void AddJsonNodeValueWithoutSerialization(string nodeJson)
    {
        var nodeValue = JsonNode.Parse(nodeJson);

        var result = JsonHelper.AddProperty(EmptyJson, "data", nodeValue);

        var resultObj = JsonNode.Parse(result)!.AsObject();
        var dataObj = resultObj["data"]!.AsObject();
        Equal(123, dataObj["id"]!.GetValue<int>());
        Equal("Test", dataObj["name"]!.GetValue<string>());
    }

    private class Address
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
    }

    private class Contact
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    #endregion
}
