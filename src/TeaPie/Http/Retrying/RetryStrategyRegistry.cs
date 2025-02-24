using Polly.Retry;

namespace TeaPie.Http.Retrying;

internal interface IRetryStrategyRegistry : IRegistry<RetryStrategyOptions<HttpResponseMessage>>;

internal class RetryStrategyRegistry : IRetryStrategyRegistry
{
    private readonly Dictionary<string, RetryStrategyOptions<HttpResponseMessage>> _registry =
        new() { { string.Empty, new() { Name = string.Empty } } };

    public void Register(string name, RetryStrategyOptions<HttpResponseMessage> retryStrategy)
        => _registry.Add(name, retryStrategy);

    public RetryStrategyOptions<HttpResponseMessage> Get(string name) => _registry[name];

    public bool IsRegistered(string name) => _registry.ContainsKey(name);
}
