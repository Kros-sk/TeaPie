using Polly.Retry;
using TeaPie.Http.Retrying;
using static Xunit.Assert;

namespace TeaPie.Tests.Http.Retrying;

[Collection(nameof(NonParallelCollection))]
public class TeaPieRetryingExtensionsShould
{
    [Fact]
    public void ThrowExceptionWhenRegisteringRetryStrategyWithNullName()
    {
        var registry = new RetryStrategyRegistry();
        var teaPie = PrepareTeaPieInstance(registry);
        var retryStrategy = new RetryStrategyOptions<HttpResponseMessage> { Name = null };

        var exception = Throws<InvalidOperationException>(() => teaPie.RegisterRetryStrategy(null!, retryStrategy));

        Equal("Unable to register retry strategy with 'null' name.", exception.Message);
    }

    [Fact]
    public void RegisterRetryStrategyWhenValidStrategyProvidedWithoutAnyProblems()
    {
        var registry = new RetryStrategyRegistry();
        var teaPie = PrepareTeaPieInstance(registry);
        var retryStrategy = new RetryStrategyOptions<HttpResponseMessage> { Name = "TestRetry" };

        teaPie.RegisterRetryStrategy("TestRetry", retryStrategy);

        True(registry.IsRegistered("TestRetry"));
    }

    private static TeaPie PrepareTeaPieInstance(IRetryStrategyRegistry retryStrategyRegistry)
        => new TeaPieBuilder().WithService(retryStrategyRegistry).Build();
}
