using Microsoft.Extensions.DependencyInjection;
using TeaPie.Http.Auth;
using TeaPie.Http.Headers;
using TeaPie.Http.Retrying;

namespace TeaPie.Http;

internal static class Setup
{
    public static IServiceCollection AddHttp(this IServiceCollection services)
    {
        services.AddScoped<IRequestExecutionContextAccessor, RequestExecutionContextAccessor>();
        services.AddScoped<ExecuteRequestStep>();
        services.AddHeaders();
        services.AddRetrying();
        services.AddAuthentication();

        return services;
    }
}
