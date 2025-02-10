using Polly;
using Polly.Retry;

namespace TeaPie.Http.Retrying;

internal interface IRetryingPoliciesRegistry
{
    void RegisterPolicy(string name, RetryStrategyOptions<HttpResponseMessage> retryStrategy);

    ResiliencePipeline<HttpResponseMessage> GetResiliencePipeline(string name);
}

internal class RetryingStrategiesRegistry : IRetryingPoliciesRegistry
{
    private readonly Dictionary<string, ResiliencePipeline<HttpResponseMessage>> _registry =
        new()
        {
            { string.Empty, ResiliencePipeline<HttpResponseMessage>.Empty }
        };

    public void RegisterPolicy(string name, RetryStrategyOptions<HttpResponseMessage> retryStrategy)
    {
        var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(retryStrategy)
            .Build();

        _registry.Add(name, pipeline);
    }

    public ResiliencePipeline<HttpResponseMessage> GetResiliencePipeline(string name) => _registry[name];
}
