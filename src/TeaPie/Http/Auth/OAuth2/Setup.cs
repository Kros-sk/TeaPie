using Microsoft.Extensions.DependencyInjection;

namespace TeaPie.Http.Auth.OAuth2;

internal static class Setup
{
    public static IServiceCollection AddOAuth2(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddHttpClient<OAuth2Provider>();

        return services;
    }
}
