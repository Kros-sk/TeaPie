using Microsoft.Extensions.DependencyInjection;
using TeaPie.Http.Headers;
using TeaPie.Http.Parsing;
using TeaPie.Http.Retrying;

namespace TeaPie.Http;

internal static class Setup
{
    public static IServiceCollection AddHttp(this IServiceCollection services)
    {
        services.AddHttpClient<HttpRequestHeadersProvider>();

        services.AddScoped<IRequestExecutionContextAccessor, RequestExecutionContextAccessor>();

        services.AddSingleton<IHeadersHandler, HeadersHandler>();
        services.AddSingleton<IHttpRequestParser, HttpRequestParser>();
        services.AddSingleton<IHttpRequestHeadersProvider, HttpRequestHeadersProvider>();

        services.AddSingleton<IRetryStrategiesRegistry, RetryStrategiesRegistry>();
        services.AddSingleton<IRetryingHandler, RetryingHandler>();

        return services;
    }
}
