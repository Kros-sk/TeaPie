namespace TeaPie.Http.Auth.OAuth2;

public class OAuth2Options : IAuthOptions
{
    public string AuthUrl { get; } = string.Empty;
    public string? GrantType { get; }
    public string? ClientId { get; }
    public string? Username { get; }
    public string? Password { get; }
    public string? ClientSecret { get; }
    public Uri? RedirectUri { get; }
    public IReadOnlyDictionary<string, string> AdditionalParameters { get; } = new Dictionary<string, string>();

    private IReadOnlyDictionary<string, string>? _cachedParameters;
    private static readonly Dictionary<string, Func<OAuth2Options, string?>> _parametersBinding = new()
    {
        { "grant_type", o => o.GrantType },
        { "client_id", o => o.ClientId },
        { "username", o => o.Username },
        { "password", o => o.Password },
        { "client_secret", o => o.ClientSecret },
        { "redirect_uri", o => o.RedirectUri?.ToString() }
    };

    internal OAuth2Options() { }

    internal OAuth2Options(
        string oauthUrl,
        string? grantType = null,
        string? clientId = null,
        string? username = null,
        string? password = null,
        string? clientSecret = null,
        Dictionary<string, string>? additionalParameters = null)
    {
        AuthUrl = oauthUrl ?? throw new ArgumentNullException(nameof(oauthUrl));
        GrantType = grantType;
        ClientId = clientId;
        Username = username;
        Password = password;
        ClientSecret = clientSecret;
        AdditionalParameters = new Dictionary<string, string>(additionalParameters ?? []);
    }

    internal bool HasParameter(string parameterName) => GetParametersAsReadOnly().ContainsKey(parameterName);

    internal string GetParameter(string parameterName) => GetParametersAsReadOnly()[parameterName];

    internal IReadOnlyDictionary<string, string> GetParametersAsReadOnly()
    {
        if (_cachedParameters is not null)
        {
            return _cachedParameters;
        }

        var parameters = new Dictionary<string, string>();

        foreach (var (key, valueSelector) in _parametersBinding)
        {
            var value = valueSelector(this);
            if (!string.IsNullOrEmpty(value))
            {
                parameters[key] = value;
            }
        }

        foreach (var parameter in AdditionalParameters)
        {
            parameters.Add(parameter.Key, parameter.Value);
        }

        _cachedParameters = parameters;
        return _cachedParameters;
    }
}
