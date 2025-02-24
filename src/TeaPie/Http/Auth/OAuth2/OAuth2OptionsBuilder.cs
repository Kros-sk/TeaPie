using System.Diagnostics.CodeAnalysis;

namespace TeaPie.Http.Auth.OAuth2;

public sealed class OAuth2OptionsBuilder
{
    private string _authUrl = string.Empty;
    private string _grantType = string.Empty;
    private string? _clientId;
    private string? _clientSecret;
    private string? _username;
    private string? _password;
    private readonly Dictionary<string, string> _additionalParameters = [];

    private OAuth2OptionsBuilder() { }

    public static OAuth2OptionsBuilder Create() => new();

    public OAuth2OptionsBuilder WithAuthUrl(string oauthUrl)
    {
        _authUrl = oauthUrl;
        return this;
    }

    public OAuth2OptionsBuilder WithGrantType(string grantType)
    {
        _grantType = grantType;
        return this;
    }

    public OAuth2OptionsBuilder WithClientId(string clientId)
    {
        _clientId = clientId;
        return this;
    }

    public OAuth2OptionsBuilder WithClientSecret(string clientSecret)
    {
        _clientSecret = clientSecret;
        return this;
    }

    public OAuth2OptionsBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    public OAuth2OptionsBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    public OAuth2OptionsBuilder AddParameter(string key, string value)
    {
        if (!string.IsNullOrWhiteSpace(key))
        {
            _additionalParameters[key] = value;
        }
        return this;
    }

    public OAuth2Options Build()
    {
        CheckRequiredParameters();
        return new(
            _authUrl,
            _grantType,
            _clientId,
            _username,
            _password,
            _clientSecret,
            _additionalParameters.Count > 0 ? new Dictionary<string, string>(_additionalParameters) : null);
    }

    [MemberNotNull(nameof(_authUrl))]
    [MemberNotNull(nameof(_grantType))]
    private void CheckRequiredParameters()
    {
        if (_authUrl is null)
        {
            throw new InvalidOperationException("Unable to create OAuth2 options, because " +
                $"'{nameof(OAuth2Options.AuthUrl)}' is required parameter.");
        }

        if (_grantType is null)
        {
            throw new InvalidOperationException("Unable to create OAuth2 options, because " +
                $"'{nameof(OAuth2Options.GrantType)}' is required parameter.");
        }
    }
}
