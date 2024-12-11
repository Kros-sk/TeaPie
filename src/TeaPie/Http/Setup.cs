using Microsoft.Extensions.DependencyInjection;

namespace TeaPie.Http;

internal static class Setup
{
    public static IServiceCollection AddHttp(this IServiceCollection services)
    {
        services.AddHttpClient<HttpRequestHeadersProvider>();

        services.AddScoped<IRequestExecutionContextAccessor, RequestExecutionContextAccessor>();

        services.AddSingleton<IHeadersResolver, HeadersResolver>();
        services.AddSingleton<IHttpRequestParser, HttpRequestParser>();
        services.AddSingleton<IHttpRequestHeadersProvider, HttpRequestHeadersProvider>();

        return services;
    }
}
