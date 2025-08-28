using Microsoft.Extensions.DependencyInjection;
using TeaPie.Environments;
using TeaPie.Http;
using TeaPie.Logging;
using TeaPie.Pipelines;
using TeaPie.Reporting;
using TeaPie.Scripts;
using TeaPie.StructureExploration;
using TeaPie.TestCases;
using TeaPie.Testing;
using TeaPie.Variables;

namespace TeaPie;

internal static class Setup
{
    public static IServiceCollection AddTeaPie(
        this IServiceCollection services, bool isCollectionRun, Action loggingConfiguration)
    {
        services.AddLogging(loggingConfiguration);
        services.AddStructureExploration(isCollectionRun);
        services.AddHttp();
        services.AddEnvironments();
        services.AddTestCases();
        services.AddScripts();
        services.AddVariables();
        services.AddTesting();
        services.AddPipelines();
        services.AddReporting();

        return services;
    }
}
