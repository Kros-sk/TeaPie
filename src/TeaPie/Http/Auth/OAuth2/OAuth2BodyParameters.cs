namespace TeaPie.Http.Auth.OAuth2;

public class OAuth2BodyParameters
{
    // Body parameters list according to RFC 6749
    public static readonly HashSet<string> SupportedBodyParameters =
    [
        "client_id",
        "client_secret",
        "grant_type",
        "redirect_uri",
        "code",
        "scope",
        "state"
    ];

    private readonly Dictionary<string, string> _parameters = [];

    public bool HasParameter(string key) => _parameters.ContainsKey(key);

    public void SetParameter(string key, string value)
    {
        if (!SupportedBodyParameters.Contains(key))
        {
            throw new InvalidOperationException($"Unable to set body parameter '{key}' for authentication request, since it" +
                "is not supported by 'RFC 6749'.");
        }
        _parameters.Add(key, value);
    }

    public string GetParameter(string key) => _parameters[key];

    public IReadOnlyDictionary<string, string> AsReadOnlyDictionary() => _parameters;
}
