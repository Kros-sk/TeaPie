using Microsoft.Extensions.DependencyInjection;
using TeaPie.Http.Auth.OAuth2;

namespace TeaPie.Http.Auth;

internal static class Setup
{
    public static IServiceCollection AddAuthentication(this IServiceCollection services)
    {
        var defaultAuthProviderAccessor = new CurrentAndDefaultAuthProviderAccessor();

        services.AddHttpClient<ExecuteRequestStep>()
            .AddHttpMessageHandler(_ => new AuthHttpMessageHandler(defaultAuthProviderAccessor));

        services.AddSingleton<IAuthProviderRegistry, AuthProviderRegistry>();
        services.AddSingleton<ICurrentAndDefaultAuthProviderAccessor>(defaultAuthProviderAccessor);

        services.AddOAuth2();

        return services;
    }
}
