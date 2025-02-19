using Microsoft.Extensions.DependencyInjection;
using TeaPie.Http.Auth;
using TeaPie.Http.Headers;
using TeaPie.Http.Parsing;
using TeaPie.Http.Retrying;

namespace TeaPie.Http;

internal static class Setup
{
    public static IServiceCollection AddHttp(this IServiceCollection services)
    {
        services.AddScoped<IRequestExecutionContextAccessor, RequestExecutionContextAccessor>();

        services.AddHttpClient<HttpRequestHeadersProvider>();
        services.AddSingleton<IHeadersHandler, HeadersHandler>();
        services.AddSingleton<IHttpRequestParser, HttpRequestParser>();
        services.AddSingleton<IHttpRequestHeadersProvider, HttpRequestHeadersProvider>();

        var defaultAuthProviderAccessor = new DefaultAuthProviderAccessor();

        services.AddHttpClient<ExecuteRequestStep>()
            .AddHttpMessageHandler(_ => new AuthHttpMessageHandler(defaultAuthProviderAccessor));

        services.AddSingleton<IRetryStrategyRegistry, RetryStrategyRegistry>();
        services.AddSingleton<IResiliencePipelineProvider, ResiliencePipelineProvider>();

        services.AddSingleton<IAuthProviderRegistry, AuthProviderRegistry>();
        services.AddSingleton<IDefaultAuthProviderAccessor>(defaultAuthProviderAccessor);

        return services;
    }
}
