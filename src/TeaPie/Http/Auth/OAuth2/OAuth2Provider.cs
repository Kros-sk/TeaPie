using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using TeaPie.Http.Headers;
using TeaPie.Variables;

namespace TeaPie.Http.Auth.OAuth2;

internal class OAuth2Provider(
    IHttpClientFactory clientFactory,
    IMemoryCache memoryCache,
    ILogger<OAuth2Provider> logger,
    IVariables variables)
    : IAuthProvider<OAuth2Options>
{
    private readonly string _accessTokenCacheKey = Guid.NewGuid() + "-access_token";
    private const string RedirectUriParameterKey = "redirect_uri";

    private readonly IHttpClientFactory _httpClientFactory = clientFactory;
    private readonly IMemoryCache _cache = memoryCache;
    private readonly ILogger<OAuth2Provider> _logger = logger;
    private readonly IVariables _variables = variables;

    private readonly AuthorizationHeaderHandler _authorizationHeaderHandler = new();
    private OAuth2Options _configuration = new();

    public async Task Authenticate(HttpRequestMessage request, CancellationToken cancellationToken)
        => _authorizationHeaderHandler.SetHeader($"Bearer {await GetToken()}", request);

    public IAuthProvider<OAuth2Options> ConfigureOptions(OAuth2Options configuration)
    {
        _configuration = configuration;
        _cache.Remove(_accessTokenCacheKey);
        return this;
    }

    private async Task<string> GetToken()
    {
        var source = "cache";
        var token = await _cache.GetOrCreateAsync(_accessTokenCacheKey, async _ =>
        {
            var newToken = await GetTokenFromRequest();
            source = ResolveRequestUri();
            if (_configuration.AccessTokenVariableName is not null)
            {
                _variables.SetVariable(_configuration.AccessTokenVariableName, newToken);
            }
            return newToken;
        })!;

        _logger.LogTrace("{Subject} was fetched from {Source}.", "Access token", source);
        return token!;
    }

    private async Task<string> GetTokenFromRequest()
    {
        ResolveParameters(out var requestContent, out var requestUri);

        LogSendingRequest();

        var result = await SendRequest(requestContent, requestUri);

        CacheToken(result);

        return result.AccessToken!;
    }

    private void LogSendingRequest()
    {
        var content = string.Join(
            Environment.NewLine, _configuration.GetParametersAsReadOnly().Select(ToStringMaskingSecrets));

        _logger.LogTrace("Content (www-url-encoded) for the following HTTP request: {NewLine}{Body}",
            Environment.NewLine,
            content);
    }

    private static string ToStringMaskingSecrets(KeyValuePair<string, string> p)
        => p.Key.Contains("password", StringComparison.OrdinalIgnoreCase) ||
            p.Key.Contains("secret", StringComparison.OrdinalIgnoreCase)
                ? $"{p.Key}={new string('*', p.Value.Length)}"
                : $"{p.Key}={p.Value}";

    private void ResolveParameters(out FormUrlEncodedContent requestContent, out string requestUri)
    {
        requestContent = new FormUrlEncodedContent(_configuration.GetParametersAsReadOnly());
        requestUri = ResolveRequestUri();
    }

    private async Task<OAuth2TokenResponse> SendRequest(FormUrlEncodedContent requestContent, string requestUri)
    {
        using var client = _httpClientFactory.CreateClient(nameof(IAuthProvider<OAuth2Options>));
        var response = await client.PostAsync(requestUri, requestContent);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OAuth2TokenResponse>();
        if (result is null || string.IsNullOrEmpty(result.AccessToken))
        {
            throw new UnauthorizedAccessException("Failed to retrieve access token.");
        }

        return result;
    }

    private void CacheToken(OAuth2TokenResponse result)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(result.ExpiresIn),
            Priority = CacheItemPriority.High
        };

        _cache.Set(_accessTokenCacheKey, result.AccessToken, cacheEntryOptions);
    }

    private string ResolveRequestUri()
        => _configuration.HasParameter(RedirectUriParameterKey)
             ? _configuration.GetParameter(RedirectUriParameterKey)
             : _configuration.AuthUrl;
}

internal class OAuth2TokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
