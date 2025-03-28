using Microsoft.Extensions.DependencyInjection;

namespace TeaPie.Scripts;

internal static class Setup
{
    public static IServiceCollection AddScripts(this IServiceCollection services)
    {
        services.AddSingleton<IScriptPreProcessor, ScriptPreProcessor>();
        services.AddSingleton<IScriptCompiler, ScriptCompiler>();
        services.AddSingleton<INuGetPackageHandler, NuGetPackageHandler>();

        services.AddSingleton<IScriptLineResolversProvider, ScriptLineResolversProvider>();

        services.AddScoped<IScriptExecutionContextAccessor, ScriptExecutionContextAccessor>();

        services.AddHttpClient<ExecuteScriptStep>();

        return services;
    }
}
