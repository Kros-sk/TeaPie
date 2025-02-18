using Microsoft.Extensions.Logging;
using NSubstitute;
using Polly.Retry;
using TeaPie.Http.Retrying;
using TeaPie.Pipelines;
using TeaPie.Reporting;
using TeaPie.TestCases;
using TeaPie.Testing;
using TeaPie.Variables;
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

        True(registry.IsRetryStrategyRegistered("TestRetry"));
    }

    private static TeaPie PrepareTeaPieInstance(IRetryStrategyRegistry retryStrategyRegistry)
        => TeaPie.Create(
            Substitute.For<IVariables>(),
            Substitute.For<ILogger>(),
            Substitute.For<ITester>(),
            Substitute.For<ICurrentTestCaseExecutionContextAccessor>(),
             new ApplicationContextBuilder().Build(),
            Substitute.For<IPipeline>(),
            Substitute.For<ITestResultsSummaryReporter>(),
            retryStrategyRegistry);
}
