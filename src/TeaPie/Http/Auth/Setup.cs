using Microsoft.Extensions.DependencyInjection;
using TeaPie.Http.Auth.OAuth2;

namespace TeaPie.Http.Auth;

internal static class Setup
{
    public static IServiceCollection AddAuthentication(this IServiceCollection services)
    {
        var defaultAuthProviderAccessor = new DefaultAuthProviderAccessor();

        services.AddHttpClient<ExecuteRequestStep>()
            .AddHttpMessageHandler(serviceProvider => new AuthHttpMessageHandler(defaultAuthProviderAccessor, serviceProvider));

        services.AddSingleton<IAuthProviderRegistry, AuthProviderRegistry>();
        services.AddSingleton<IDefaultAuthProviderAccessor>(defaultAuthProviderAccessor);

        services.AddOAuth2();

        return services;
    }
}
