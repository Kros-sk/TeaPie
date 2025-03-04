using Microsoft.Extensions.DependencyInjection;

namespace TeaPie.StructureExploration;

internal static class Setup
{
    public static IServiceCollection AddStructureExploration(
        this IServiceCollection services, IApplicationAbstractFactory applicationAbstractFactory)
        => applicationAbstractFactory.AddStructureExploration(services);
}
