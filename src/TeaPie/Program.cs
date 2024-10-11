using TeaPie.Pipelines.Application;
using TeaPie.Pipelines.StructureExploration;

if (args.Length > 0)
{
    var pipeline = new ApplicationPipeline();
    var context = new ApplicationContext(args[0]);
    pipeline.AddStep(new StructureExplorationStep());

    await pipeline.RunAsync(context);
}


