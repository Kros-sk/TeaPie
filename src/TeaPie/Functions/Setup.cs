using Microsoft.Extensions.DependencyInjection;

namespace TeaPie.Functions;

internal static class Setup
{
    public static IServiceCollection AddFunctions(this IServiceCollection services)
    {
        services.AddSingleton<IFunctions, Functions>();
        services.AddSingleton<IFunctionsResolver, FunctionsResolver>();

        return services;
    }
}
