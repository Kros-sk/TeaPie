using Polly.Retry;

namespace TeaPie.Http.Retrying;

public static class TeaPieRetryingExtensions
{
    public static void RegisterRetryStrategy(
        this TeaPie teaPie, string name, RetryStrategyOptions<HttpResponseMessage> retryStrategy)
        => teaPie._retryingStrategiesRegistry.RegisterStrategy(name, retryStrategy);
}
