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
    private string? _accessTokenVariableName;
    private readonly Dictionary<string, string> _additionalParameters = [];

    private OAuth2OptionsBuilder() { }

    public static OAuth2OptionsBuilder Create() => new();

    /// <summary>
    /// Adds URL to which user should be authenticated.
    /// </summary>
    /// <param name="oauthUrl">URL to which user should be authenticated.</param>
    /// <returns>Updated instance of builder.</returns>
    public OAuth2OptionsBuilder WithAuthUrl(string oauthUrl)
    {
        _authUrl = oauthUrl;
        return this;
    }

    /// <summary>
    /// Specifies which grant type should be used.
    /// </summary>
    /// <param name="grantType">Grant type to be used for authentication.</param>
    /// <returns>Updated instance of builder.</returns>
    public OAuth2OptionsBuilder WithGrantType(string grantType)
    {
        _grantType = grantType;
        return this;
    }

    /// <summary>
    /// Adds 'clientId' parameter with <paramref name="clientId"/> value.
    /// </summary>
    /// <param name="clientId">Value of the parameter with name 'clientId'.</param>
    /// <returns>Updated instance of builder.</returns>
    public OAuth2OptionsBuilder WithClientId(string clientId)
    {
        _clientId = clientId;
        return this;
    }

    /// <summary>
    /// Adds 'clientSecret' parameter with <paramref name="clientSecret"/> value.
    /// </summary>
    /// <param name="clientSecret">Value of the parameter with name 'clientSecret'.</param>
    /// <returns>Updated instance of builder.</returns>
    public OAuth2OptionsBuilder WithClientSecret(string clientSecret)
    {
        _clientSecret = clientSecret;
        return this;
    }

    /// <summary>
    /// Adds 'username' parameter with <paramref name="username"/> value.
    /// </summary>
    /// <param name="username">Value of the parameter with name 'username'.</param>
    /// <returns>Updated instance of builder.</returns>
    public OAuth2OptionsBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    /// <summary>
    /// Adds 'password' parameter with <paramref name="password"/> value.
    /// With refresh of the token, variable will be updated.
    /// </summary>
    /// <param name="password">Value of the parameter with name 'password'.</param>
    /// <returns>Updated instance of builder.</returns>
    public OAuth2OptionsBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    /// <summary>
    /// Access token will be saved to variable named by <paramref name="variableName"/>.
    /// With refresh of the token, variable will be updated.
    /// </summary>
    /// <param name="variableName">Name of the variable which will hold access token value.</param>
    /// <returns>Updated instance of builder.</returns>
    public OAuth2OptionsBuilder WithAccessTokenVariableName(string variableName)
    {
        _accessTokenVariableName = variableName;
        return this;
    }

    /// <summary>
    /// Adds an additional parameter with <paramref name="key"/> and <paramref name="value"/>.
    /// </summary>
    /// <param name="key">The key of the additional parameter.</param>
    /// <param name="value">The value of the additional parameter.</param>
    /// <returns>Updated instance of builder.</returns>
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
            _accessTokenVariableName,
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
