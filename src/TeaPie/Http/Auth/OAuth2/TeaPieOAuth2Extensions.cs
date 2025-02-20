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
        var provider = teaPie._authenticationProviderRegistry.GetAuthProvider(AuthConstants.OAuth2Key);
        ((OAuth2Provider)provider).ConfigureOptions(options);
    }
}
