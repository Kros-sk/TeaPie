using Polly.Retry;

namespace TeaPie.Http.Retrying;

public static class TeaPieRetryingExtensions
{
    public static void RegisterRetryStrategy(
        this TeaPie teaPie, string name, RetryStrategyOptions<HttpResponseMessage> retryStrategy)
    {
        CheckAndResolveRealNameOfRetryStrategy(name, retryStrategy, out var realName);

        teaPie._retryingStrategiesRegistry.RegisterStrategy(realName, retryStrategy);
    }

    private static void CheckAndResolveRealNameOfRetryStrategy(
        string name,
        RetryStrategyOptions<HttpResponseMessage> retryStrategy,
        out string realName)
    {
        if (retryStrategy.Name is null)
        {
            throw new InvalidOperationException("Unable to register retry strategy with 'null' name.");
        }

        if (retryStrategy.Name.Equals(RetryingConstants.DefaultName) && name?.Equals(RetryingConstants.DefaultName) == false)
        {
            retryStrategy.Name = name;
        }

        realName = retryStrategy.Name;
    }
}
