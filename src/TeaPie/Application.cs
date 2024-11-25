using TeaPie.Pipelines.Application;

namespace TeaPie;

public sealed class Application
{
    private readonly ApplicationPipeline _pipeline;
    private readonly ApplicationContext _appContext;

    internal Application(ApplicationPipeline pipeline, ApplicationContext applicationContext)
    {
        _pipeline = pipeline;
        _appContext = applicationContext;
    }

    public async Task Run(CancellationToken cancellationToken = default)
        => await _pipeline.Run(_appContext, cancellationToken);
}
