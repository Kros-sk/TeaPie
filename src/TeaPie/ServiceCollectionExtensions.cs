using Microsoft.Extensions.DependencyInjection;
using TeaPie.ScriptHandling;
using TeaPie.StructureExploration;

namespace TeaPie;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddSingleton<IStructureExplorer, StructureExplorer>();
        services.AddSingleton<IScriptPreProcessor, ScriptPreProcessor>();
        return services;
    }
}
