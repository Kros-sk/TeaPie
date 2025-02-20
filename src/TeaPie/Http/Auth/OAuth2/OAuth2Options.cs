namespace TeaPie.Http.Auth.OAuth2;

public class OAuth2Options : IAuthProviderOptions
{
    public OAuth2Options() { }

    public OAuth2Options(string authUrl, string grantType, params KeyValuePair<string, string>[] otherParameters)
    {
        OAuthUrl = authUrl;
        GrantType = grantType;
        _otherParameters = [];
        foreach (var parameter in otherParameters)
        {
            _otherParameters.Add(parameter.Key, parameter.Value);
        }
    }

    public string OAuthUrl { get; set; } = string.Empty;

    public string GrantType { get; set; } = string.Empty;

    private readonly Dictionary<string, string> _otherParameters = [];
    public IReadOnlyDictionary<string, string> OtherBodyParameters => _otherParameters;
}
