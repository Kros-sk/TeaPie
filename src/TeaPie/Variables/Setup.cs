using Microsoft.Extensions.DependencyInjection;

namespace TeaPie.Variables;
internal static class Setup
{
    public static IServiceCollection AddVariables(this IServiceCollection services)
    {
        services.AddSingleton<IVariables, Variables>();
        services.AddSingleton<IVariablesResolver, VariablesResolver>();

        return services;
    }
}
