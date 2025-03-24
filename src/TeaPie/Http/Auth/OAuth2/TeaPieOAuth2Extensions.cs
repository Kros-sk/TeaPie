using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeaPie.Variables;

namespace TeaPie.Http.Auth.OAuth2;

public static class TeaPieOAuth2Extensions
{
    /// <summary>
    /// Configures options for <b>OAuth2</b> authentication provider.
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="options">Options with which <b>OAuth2</b> authentication provider will be configured.</param>
    public static void ConfigureOAuth2Provider(this TeaPie teaPie, OAuth2Options options)
    {
        var provider = teaPie._authenticationProviderRegistry.Get(AuthConstants.OAuth2Key);
        ((OAuth2Provider)provider).ConfigureOptions(options);
    }

    /// <summary>
    /// Configures options for custom <b>OAuth2</b> authentication provider, which will be registered by <paramref name="name"/>.
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="name">Name by which new OAuth2 provider should be registered (and configured).</param>
    /// <param name="options">Options with which <b>OAuth2</b> authentication provider will be configured.</param>
    public static void ConfigureOAuth2Provider(this TeaPie teaPie, string name, OAuth2Options options)
    {
        var newProvider = new OAuth2Provider(
            teaPie._serviceProvider.GetRequiredService<IHttpClientFactory>(),
            teaPie._serviceProvider.GetRequiredService<IMemoryCache>(),
            teaPie._serviceProvider.GetRequiredService<ILogger<OAuth2Provider>>(),
            teaPie._serviceProvider.GetRequiredService<IVariables>());

        teaPie._authenticationProviderRegistry.Register(name, newProvider);
        newProvider.ConfigureOptions(options);
    }

    /// <summary>
    /// Sets <b>OAuth2</b> authentication provider as default.
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    public static void SetOAuth2AsDefaultAuthProvider(this TeaPie teaPie)
        => teaPie.SetDefaultAuthProvider(AuthConstants.OAuth2Key);
}
