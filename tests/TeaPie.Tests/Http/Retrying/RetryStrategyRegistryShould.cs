using Polly.Retry;
using TeaPie.Http.Retrying;
using static Xunit.Assert;

namespace TeaPie.Tests.Http.Retrying;

public class RetryStrategyRegistryShould
{
    private readonly RetryStrategyRegistry _retryStrategyRegistry;

    public RetryStrategyRegistryShould()
    {
        _retryStrategyRegistry = new RetryStrategyRegistry();
    }

    [Fact]
    public void ThrowExceptionWhenRegisteringRetryStrategyWithNullName()
    {
        var retryStrategy = new RetryStrategyOptions<HttpResponseMessage> { Name = null };

        var exception = Throws<ArgumentNullException>(() => _retryStrategyRegistry.RegisterRetryStrategy(null!, retryStrategy));

        Equal("Value cannot be null. (Parameter 'key')", exception.Message);
    }

    [Fact]
    public void RegisterRetryStrategyWhenValidStrategyProvided()
    {
        var retryStrategy = new RetryStrategyOptions<HttpResponseMessage> { Name = "TestRetry" };

        _retryStrategyRegistry.RegisterRetryStrategy("TestRetry", retryStrategy);

        True(_retryStrategyRegistry.IsRetryStrategyRegistered("TestRetry"));
    }
}
