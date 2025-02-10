using Polly.Retry;

namespace TeaPie.Http.Retrying;

public static class TeaPieRetryingExtensions
{
    public static void RegisterRetryStrategy(this TeaPie teaPie, string name, RetryStrategyOptions retryStrategy)
        => teaPie._retryingStrategiesRegistry.RegisterStrategy(name, retryStrategy);
}
