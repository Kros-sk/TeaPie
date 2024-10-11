using Microsoft.Extensions.DependencyInjection;
using TeaPie.Pipelines.Application;
using TeaPie.Pipelines.StructureExploration;
using TeaPie.StructureExploration;

if (args.Length > 0)
{
    var services = new ServiceCollection();
    services.AddSingleton<IStructureExplorer, StructureExplorer>();

    var serviceProvider = services.BuildServiceProvider();

    var pipeline = new ApplicationPipeline();
    var context = new ApplicationContext(args[0]);
    pipeline.AddStep(StructureExplorationStep.Create(serviceProvider));

    await pipeline.RunAsync(context);
}


