using Microsoft.Extensions.DependencyInjection;
using TeaPie.StructureExploration;

namespace TeaPie;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddSingleton<IStructureExplorer, StructureExplorer>();
        return services;
    }
}
