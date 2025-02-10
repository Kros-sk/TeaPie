using Polly;
using Polly.Retry;

namespace TeaPie.Http.Retrying;

internal interface IRetryingStrategiesRegistry
{
    void RegisterStrategy(string name, RetryStrategyOptions<HttpResponseMessage> retryStrategy);

    ResiliencePipeline<HttpResponseMessage> GetStrategy(string name);
}

internal class RetryingStrategiesRegistry : IRetryingStrategiesRegistry
{
    private readonly Dictionary<string, ResiliencePipeline<HttpResponseMessage>> _registry =
        new()
        {
            { string.Empty, ResiliencePipeline<HttpResponseMessage>.Empty }
        };

    public void RegisterStrategy(string name, RetryStrategyOptions<HttpResponseMessage> retryStrategy)
    {
        var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(retryStrategy)
            .Build();

        _registry.Add(name, pipeline);
    }

    public ResiliencePipeline<HttpResponseMessage> GetStrategy(string name) => _registry[name];
}
