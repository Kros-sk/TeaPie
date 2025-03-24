using Microsoft.Extensions.DependencyInjection;
using TeaPie.Http.Auth.OAuth2;
using TeaPie.Logging;

namespace TeaPie.Http.Auth;

internal static class Setup
{
    public static IServiceCollection AddAuthentication(this IServiceCollection services)
    {
        services.AddTransient<AuthHttpMessageHandler>();

        services.AddHttpClient<ExecuteRequestStep>()
            .AddHttpMessageHandler<AuthHttpMessageHandler>()
            .AddHttpMessageHandler<LoggingInterceptorHandler>();

        services.AddSingleton<IAuthProviderRegistry, AuthProviderRegistry>();
        services.AddSingleton<IAuthProviderAccessor, AuthProviderAccessor>();

        services.AddOAuth2();

        return services;
    }
}
