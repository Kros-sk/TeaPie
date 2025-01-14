using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace TeaPie.Json;

public class CaseInsensitiveExpandoObject : DynamicObject
{
    private readonly IDictionary<string, object?> _properties;

    public CaseInsensitiveExpandoObject(IDictionary<string, object?> dictionary)
    {
        _properties = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in dictionary)
        {
            _properties[kvp.Key] = kvp.Value switch
            {
                JObject jObject => new CaseInsensitiveExpandoObject(jObject.ToObject<Dictionary<string, object?>>() ?? []),
                JArray jArray => ConvertToList(jArray),
                _ => kvp.Value
            };
        }
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
        => _properties.TryGetValue(binder.Name, out result);

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        _properties[binder.Name] = value;
        return true;
    }

    public override IEnumerable<string> GetDynamicMemberNames()
        => _properties.Keys;

    private static List<object> ConvertToList(JArray jArray)
    {
        var list = new List<object>(jArray.Count);
        foreach (var item in jArray)
        {
            list.Add(item switch
            {
                JObject jObject => new CaseInsensitiveExpandoObject(jObject.ToObject<Dictionary<string, object?>>() ?? []),
                _ => item
            });
        }
        return list;
    }
}
