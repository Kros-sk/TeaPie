using Microsoft.Extensions.DependencyInjection;

namespace TeaPie.Environments;

internal static class Setup
{
    public static IServiceCollection AddEnvironments(this IServiceCollection services)
        => services.AddSingleton<IEnvironmentsRegistry, EnvironmentsRegistry>();
}
