using Microsoft.Extensions.DependencyInjection;
using TeaPie.Http.Auth.OAuth2;

namespace TeaPie.Http.Auth;

internal static class Setup
{
    public static IServiceCollection AddAuthentication(this IServiceCollection services)
    {
        var defaultAuthProviderAccessor = new AuthProviderAccessor();

        services.AddHttpClient<ExecuteRequestStep>()
            .AddHttpMessageHandler(_ => new AuthHttpMessageHandler(defaultAuthProviderAccessor));

        services.AddSingleton<IAuthProviderRegistry, AuthProviderRegistry>();
        services.AddSingleton<IAuthProviderAccessor>(defaultAuthProviderAccessor);

        services.AddOAuth2();

        return services;
    }
}
