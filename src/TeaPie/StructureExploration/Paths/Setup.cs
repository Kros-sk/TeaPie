using Microsoft.Extensions.DependencyInjection;

namespace TeaPie.StructureExploration.Paths;

internal static class Setup
{
    public static IServiceCollection AddPaths(this IServiceCollection services)
    {
        services.AddSingleton<IPathResolver, PathResolver>();
        services.AddSingleton<IPathProvider, PathProvider>();
        services.AddSingleton<RelativePathResolver>();
        services.AddSingleton<TemporaryPathResolver>();

        return services;
    }
}
