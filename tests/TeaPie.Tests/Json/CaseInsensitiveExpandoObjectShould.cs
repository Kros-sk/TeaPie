using FluentAssertions;
using Newtonsoft.Json.Linq;
using TeaPie.Json;

namespace TeaPie.Tests.Json;

public class CaseInsensitiveExpandoObjectShould
{
    [Fact]
    public void ReturnValueForMatchingKey()
    {
        var dict = new Dictionary<string, object?> { { "Name", "John" } };
        dynamic obj = new CaseInsensitiveExpandoObject(dict);

        string name = obj.Name;

        name.Should().Be("John");
    }

    [Fact]
    public void ReturnValueCaseInsensitively()
    {
        var dict = new Dictionary<string, object?> { { "Name", "John" } };
        dynamic obj = new CaseInsensitiveExpandoObject(dict);

        string name = obj.name;

        name.Should().Be("John");
    }

    [Fact]
    public void SetValueViaDynamicMember()
    {
        var dict = new Dictionary<string, object?> { { "Name", "John" } };
        dynamic obj = new CaseInsensitiveExpandoObject(dict);

        obj.Name = "Jane";
        string name = obj.Name;

        name.Should().Be("Jane");
    }

    [Fact]
    public void ReturnKeysFromGetDynamicMemberNames()
    {
        var dict = new Dictionary<string, object?> { { "Key1", "A" }, { "Key2", "B" } };
        var obj = new CaseInsensitiveExpandoObject(dict);

        var members = obj.GetDynamicMemberNames();

        members.Should().Contain("Key1").And.Contain("Key2");
    }

    [Fact]
    public void WrapJObjectAsCaseInsensitiveExpandoObject()
    {
        var jObj = JObject.FromObject(new { inner = "value" });
        var dict = new Dictionary<string, object?> { { "nested", jObj } };
        dynamic obj = new CaseInsensitiveExpandoObject(dict);

        object nested = obj.nested;

        nested.Should().BeOfType<CaseInsensitiveExpandoObject>();
    }

    [Fact]
    public void WrapJArrayAsList()
    {
        var jArr = new JArray(1, 2, 3);
        var dict = new Dictionary<string, object?> { { "items", jArr } };
        dynamic obj = new CaseInsensitiveExpandoObject(dict);

        object items = obj.items;

        items.Should().BeOfType<List<object>>();
    }
}
