using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using TeaPie.Http.Headers;

namespace TeaPie.Http.Auth;

public class OAuth2Provider(HttpClient httpClient, IMemoryCache memoryCache)
    : IAuthProvider
{
    private const string AccessTokenCacheKey = "access_token";
    private const string RedirectUriParameterKey = "redirect_uri";

    private readonly HttpClient _httpClient = httpClient;
    private readonly IMemoryCache _cache = memoryCache;
    private readonly AuthorizationHeaderHandler _authorizationHeaderHandler = new();
    private OAuth2Configuration _configuration = new();

    public void SetConfiguration(OAuth2Configuration configuration) => _configuration = configuration;

    public async Task Authenticate(HttpRequestMessage request, CancellationToken cancellationToken)
        => _authorizationHeaderHandler.SetHeader($"Bearer {await GetToken()}", request);

    private async Task<string> GetToken()
        => _cache.TryGetValue(AccessTokenCacheKey, out string? token)
            ? token!
            : await GetTokenFromRequest();

    private async Task<string> GetTokenFromRequest()
    {
        ResolveParameters(out var requestContent, out var requestUri);

        var result = await SendRequest(requestContent, requestUri);

        CacheToken(result);

        return result.AccessToken!;
    }

    private void ResolveParameters(out FormUrlEncodedContent requestContent, out string requestUri)
    {
        requestContent = new FormUrlEncodedContent(_configuration.BodyParameters.AsReadOnlyDictionary());
        requestUri = ResolveRequestUri();
    }

    private async Task<OAuth2TokenResponse> SendRequest(FormUrlEncodedContent requestContent, string requestUri)
    {
        var response = await _httpClient.PostAsync(requestUri, requestContent);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OAuth2TokenResponse>();
        if (result is null || string.IsNullOrEmpty(result.AccessToken))
        {
            throw new Exception("Failed to retrieve access token.");
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

        _cache.Set(AccessTokenCacheKey, result.AccessToken, cacheEntryOptions);
    }

    private string ResolveRequestUri()
        => _configuration.BodyParameters.HasParameter(RedirectUriParameterKey)
             ? _configuration.BodyParameters.GetParameter(RedirectUriParameterKey)
             : _configuration.OAuthUrl;
}

internal class OAuth2TokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

public class OAuth2Configuration
{
    public string OAuthUrl { get; set; } = string.Empty;
    public OAuth2BodyParameters BodyParameters { get; set; } = new();
}
