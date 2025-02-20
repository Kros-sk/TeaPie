using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TeaPie.Application;

namespace TeaPie.Pipelines;

internal static class Setup
{
    public static IServiceCollection AddPipelines(this IServiceCollection services)
    {
        services.AddSingleton<IPipeline, ApplicationPipeline>();

        var assembly = Assembly.GetExecutingAssembly();

        foreach (var implementation in FindImplementations<IPipelineStep>(assembly))
        {
            services.AddTransient(implementation);
        }

        return services;
    }

    private static IEnumerable<Type> FindImplementations<TInterface>(Assembly assembly)
        => assembly.GetTypes()
            .Where(t => typeof(TInterface).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);
}
