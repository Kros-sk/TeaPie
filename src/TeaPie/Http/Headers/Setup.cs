using Microsoft.Extensions.DependencyInjection;
using TeaPie.Http.Parsing;

namespace TeaPie.Http.Headers;

internal static class Setup
{
    public static IServiceCollection AddHeaders(this IServiceCollection services)
    {
        services.AddHttpClient<HttpRequestHeadersProvider>();
        services.AddSingleton<IHeadersHandler, HeadersHandler>();
        services.AddSingleton<IHttpRequestParser, HttpRequestParser>();
        services.AddSingleton<IHttpRequestHeadersProvider, HttpRequestHeadersProvider>();

        return services;
    }
}
