using Polly.Retry;

namespace TeaPie.Http.Retrying;

public static class TeaPieRetryingExtensions
{
    /// <summary>
    /// Registers <paramref name="retryStrategy"/> with given <paramref name="name"/>.
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="name">Name by which retry strategy will be registered.</param>
    /// <param name="retryStrategy">Retry strategy to be registered.</param>
    public static void RegisterRetryStrategy(
        this TeaPie teaPie, string name, RetryStrategyOptions<HttpResponseMessage> retryStrategy)
    {
        CheckAndResolveRealNameOfRetryStrategy(name, retryStrategy, out var realName);

        teaPie._retryStrategyRegistry.Register(realName, retryStrategy);
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
