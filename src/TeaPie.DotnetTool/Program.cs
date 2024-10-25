using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeaPie;
using TeaPie.Pipelines;
using TeaPie.Pipelines.Application;
using TeaPie.Pipelines.StructureExploration;
using TeaPie.Pipelines.TemporaryFolder;

var services = new ServiceCollection();
services.ConfigureServices();
var provider = services.BuildServiceProvider();

if (args.Length > 0)
{
    var logger = provider.GetRequiredService<ILogger>();
    TeaPie.TeaPie.Create(logger);

    var pipeline = new ApplicationPipeline();
    pipeline.AddSteps(
        StructureExplorationStep.Create(provider),
        PrepareTemporaryFolderStep.Create(pipeline),
        StepsGenerationStep.Create(pipeline, provider)
    );

    var context = new ApplicationContext(args[0], logger);

    await pipeline.Run(context);
    Console.WriteLine();
}
