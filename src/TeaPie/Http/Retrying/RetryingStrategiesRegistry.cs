using Polly;
using Polly.Retry;

namespace TeaPie.Http.Retrying;

internal interface IRetryingStrategiesRegistry
{
    void RegisterStrategy(string name, RetryStrategyOptions retryStrategy);

    ResiliencePipeline GetStrategy(string name);
}

internal class RetryingStrategiesRegistry : IRetryingStrategiesRegistry
{
    private readonly Dictionary<string, ResiliencePipeline> _registry = [];

    public void RegisterStrategy(string name, RetryStrategyOptions retryStrategy)
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(retryStrategy)
            .Build();

        _registry.Add(name, pipeline);
    }

    public ResiliencePipeline GetStrategy(string name) => _registry[name];
}
