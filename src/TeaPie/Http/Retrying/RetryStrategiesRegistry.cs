using Polly.Retry;

namespace TeaPie.Http.Retrying;

internal interface IRetryStrategiesRegistry
{
    void RegisterStrategy(string name, RetryStrategyOptions<HttpResponseMessage> retryStrategy);

    RetryStrategyOptions<HttpResponseMessage> GetStrategy(string name);

    bool IsStrategyRegisterd(string name);
}

internal class RetryStrategiesRegistry : IRetryStrategiesRegistry
{
    private readonly Dictionary<string, RetryStrategyOptions<HttpResponseMessage>> _registry =
        new() { { string.Empty, new() { Name = string.Empty } } };

    public void RegisterStrategy(string name, RetryStrategyOptions<HttpResponseMessage> retryStrategy)
        => _registry.Add(name, retryStrategy);

    public RetryStrategyOptions<HttpResponseMessage> GetStrategy(string name) => _registry[name];
    public bool IsStrategyRegisterd(string name) => _registry.ContainsKey(name);
}
