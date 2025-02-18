using Polly.Retry;

namespace TeaPie.Http.Retrying;

internal interface IRetryStrategyRegistry
{
    void RegisterRetryStrategy(string name, RetryStrategyOptions<HttpResponseMessage> retryStrategy);

    RetryStrategyOptions<HttpResponseMessage> GetRetryStrategy(string name);

    bool IsRetryStrategyRegistered(string name);
}

internal class RetryStrategyRegistry : IRetryStrategyRegistry
{
    private readonly Dictionary<string, RetryStrategyOptions<HttpResponseMessage>> _registry =
        new() { { string.Empty, new() { Name = string.Empty } } };

    public void RegisterRetryStrategy(string name, RetryStrategyOptions<HttpResponseMessage> retryStrategy)
        => _registry.Add(name, retryStrategy);

    public RetryStrategyOptions<HttpResponseMessage> GetRetryStrategy(string name) => _registry[name];

    public bool IsRetryStrategyRegistered(string name) => _registry.ContainsKey(name);
}
