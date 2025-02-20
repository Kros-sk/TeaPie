using Microsoft.Extensions.DependencyInjection;

namespace TeaPie.Http.Auth.OAuth2;

internal static class Setup
{
    public static IServiceCollection AddOAuth2(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<OAuth2Provider>();
        services.AddHttpClient<IAuthProvider<OAuth2Options>, OAuth2Provider>();

        return services;
    }
}
