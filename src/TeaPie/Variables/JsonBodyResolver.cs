using Newtonsoft.Json.Linq;

namespace TeaPie.Variables;

internal class JsonBodyResolver : IBodyResolver
{
    public bool CanResolve(string mediaType) => mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase);

    public string Resolve(string body, string query, string defaultValue = "")
    {
        var json = JToken.Parse(body);
        var token = json.SelectToken(query);

        if (token is null)
        {
            return defaultValue;
        }

        return token.ToString();
    }
}
