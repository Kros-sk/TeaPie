using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeaPie.Extensions;
using TeaPie.Pipelines;
using TeaPie.Pipelines.Application;
using TeaPie.Pipelines.StructureExploration;
using TeaPie.Pipelines.TemporaryFolder;

var services = new ServiceCollection();
services.ConfigureServices();
services.ConfigureLogging();
services.ConfigureAccessors();
services.ConfigureSteps();
services.AddSingleton<IPipeline, ApplicationPipeline>();

var provider = services.BuildServiceProvider();

if (args.Length > 0)
{
    TeaPie.TeaPie.Create(provider.GetRequiredService<ILogger<TeaPie.TeaPie>>());

    var pipeline = provider.GetRequiredService<IPipeline>();
    pipeline.AddSteps(provider.GetStep<StructureExplorationStep>());
    pipeline.AddSteps(provider.GetStep<PrepareTemporaryFolderStep>());
    pipeline.AddSteps(provider.GetStep<StepsGenerationStep>());

    var context = new ApplicationContext(args[0], provider.GetRequiredService<ILogger<ApplicationContext>>(), provider);

    await pipeline.Run(context, CancellationToken.None);
}
