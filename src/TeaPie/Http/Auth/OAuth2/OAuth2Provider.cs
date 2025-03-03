using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using TeaPie.Http.Headers;

namespace TeaPie.Http.Auth.OAuth2;

internal class OAuth2Provider(IHttpClientFactory clientFactory, IMemoryCache memoryCache, ILogger<OAuth2Provider> logger)
    : IAuthProvider<OAuth2Options>
{
    private const string AccessTokenCacheKey = "access_token";
    private const string RedirectUriParameterKey = "redirect_uri";

    private readonly IHttpClientFactory _httpClientFactory = clientFactory;
    private readonly IMemoryCache _cache = memoryCache;
    private readonly ILogger<OAuth2Provider> _logger = logger;

    private readonly AuthorizationHeaderHandler _authorizationHeaderHandler = new();
    private OAuth2Options _configuration = new();

    public async Task Authenticate(HttpRequestMessage request, CancellationToken cancellationToken)
        => _authorizationHeaderHandler.SetHeader($"Bearer {await GetToken()}", request);

    public IAuthProvider<OAuth2Options> ConfigureOptions(OAuth2Options configuration)
    {
        _configuration = configuration;
        return this;
    }

    private async Task<string> GetToken()
    {
        var source = "cache";
        if (!_cache.TryGetValue(AccessTokenCacheKey, out string? token))
        {
            token = await GetTokenFromRequest();
            source = ResolveRequestUri();
        }

        _logger.LogTrace("{Subject} was fetched from {Source}.", "Access token", source);
        return token!;
    }

    private async Task<string> GetTokenFromRequest()
    {
        ResolveParameters(out var requestContent, out var requestUri);

        var result = await SendRequest(requestContent, requestUri);

        CacheToken(result);

        return result.AccessToken!;
    }

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

        _cache.Set(AccessTokenCacheKey, result.AccessToken, cacheEntryOptions);
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
