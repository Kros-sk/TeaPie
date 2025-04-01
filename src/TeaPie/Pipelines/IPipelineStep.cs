namespace TeaPie.Pipelines;

internal interface IPipelineStep
{
    bool ShouldExecute(ApplicationContext context) => context.PrematureTermination is null;

    Task Execute(ApplicationContext context, CancellationToken cancellationToken = default);
}
