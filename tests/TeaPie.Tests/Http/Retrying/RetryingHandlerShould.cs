using Polly;
using Polly.Retry;
using System.Net;
using TeaPie.Http.Retrying;
using static Xunit.Assert;

namespace TeaPie.Tests.Http.Retrying;

public class RetryingHandlerShould
{
    private readonly IRetryStrategiesRegistry _retryStrategyRegistry;
    private readonly RetryingHandler _retryingHandler;

    public RetryingHandlerShould()
    {
        _retryStrategyRegistry = new RetryStrategiesRegistry();
        _retryingHandler = new RetryingHandler(_retryStrategyRegistry);
    }

    [Fact]
    public void ThrowExceptionWhenRegisteringRetryStrategyWithoutName()
    {
        var retryStrategy = new RetryStrategyOptions<HttpResponseMessage> { Name = null };

        var exception = Throws<InvalidOperationException>(() => _retryingHandler.RegisterRetryStrategy(retryStrategy));

        Equal("Unable to register retry strategy with 'null' name.", exception.Message);
    }

    [Fact]
    public void RegisterRetryStrategyWhenValidStrategyProvided()
    {
        var retryStrategy = new RetryStrategyOptions<HttpResponseMessage> { Name = "TestRetry" };

        _retryingHandler.RegisterRetryStrategy(retryStrategy);

        True(_retryStrategyRegistry.IsStrategyRegisterd("TestRetry"));
    }

    [Fact]
    public void ThrowExceptionWhenFetchingNonexistentResiliencePipeline()
    {
        var exception = Throws<InvalidOperationException>(() => _retryingHandler.GetResiliencePipeline("NonexistentStrategy"));

        Equal("Unable to find retry strategy with name 'NonexistentStrategy'.", exception.Message);
    }

    [Fact]
    public void ReturnExistingPipelineWhenFetchingResiliencePipelineByName()
    {
        var retryStrategy = new RetryStrategyOptions<HttpResponseMessage> { Name = "ExistingRetry" };
        _retryingHandler.RegisterRetryStrategy(retryStrategy);

        var pipeline = _retryingHandler.GetResiliencePipeline("ExistingRetry");

        NotNull(pipeline);
    }

    [Fact]
    public async Task RetryUntilMatchingStatusCodeWhenUsingRetryUntilStatusCodesPipeline()
    {
        var statusCodes = new List<HttpStatusCode> { HttpStatusCode.OK, HttpStatusCode.Created };
        var pipeline = _retryingHandler.GetRetryUntilStatusCodesResiliencePipeline(statusCodes);

        var attempts = 0;
        var failingResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        var response = await pipeline.ExecuteAsync(async _ =>
        {
            attempts++;
            await Task.CompletedTask;
            return attempts == 3 ? new HttpResponseMessage(HttpStatusCode.OK) : failingResponse;
        });

        Equal(HttpStatusCode.OK, response.StatusCode);
        Equal(3, attempts);
    }

    [Fact]
    public async Task StopRetryingWhenMatchingStatusCodeIsReceived()
    {
        var statusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
        var pipeline = _retryingHandler.GetRetryUntilStatusCodesResiliencePipeline(statusCodes);

        var attempts = 0;
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

        var response = await pipeline.ExecuteAsync(async _ =>
        {
            attempts++;
            await Task.CompletedTask;
            return successResponse;
        });

        Equal(HttpStatusCode.OK, response.StatusCode);
        Equal(1, attempts);
    }

    [Fact]
    public async Task RetryMaximumNumberOfAttemptsWhenStatusCodeIsNotInList()
    {
        var statusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
        var pipeline = _retryingHandler.GetRetryUntilStatusCodesResiliencePipeline(statusCodes);

        var executionCount = 0;

        async ValueTask<HttpResponseMessage> SimulatedHttpCall(CancellationToken token)
        {
            executionCount++;
            await Task.CompletedTask;
            return new HttpResponseMessage(HttpStatusCode.NotFound); // 404 (should not retry)
        }

        var response = await pipeline.ExecuteAsync(SimulatedHttpCall, CancellationToken.None);

        Equal(HttpStatusCode.NotFound, response.StatusCode);
        Equal(RetryingConstants.DefaultRetryCount + 1, executionCount);
    }

    [Fact]
    public async Task RetryMaximumNumberOfAttemptsWhenStatusCodeIsNotInListAndRetryStrategyIsMerged()
    {
        var statusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };

        var baseStrategy = new RetryStrategyOptions<HttpResponseMessage>
        {
            Name = "BaseRetry",
            MaxRetryAttempts = 5,
            Delay = TimeSpan.FromMilliseconds(50),
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .HandleResult(response => response.StatusCode == HttpStatusCode.InternalServerError)
        };

        _retryStrategyRegistry.RegisterStrategy("BaseRetry", baseStrategy);

        var pipeline = _retryingHandler.GetRetryUntilStatusCodesResiliencePipeline(statusCodes, "BaseRetry");

        var executionCount = 0;

        async ValueTask<HttpResponseMessage> SimulatedHttpCall(CancellationToken token)
        {
            executionCount++;
            await Task.CompletedTask;
            return new HttpResponseMessage(HttpStatusCode.NotFound); // 404 (should not retry)
        }

        var response = await pipeline.ExecuteAsync(SimulatedHttpCall, CancellationToken.None);

        Equal(HttpStatusCode.NotFound, response.StatusCode);
        Equal(5 + 1, executionCount);
    }

    [Fact]
    public async Task RetryUntilStatusCodesAreMatched()
    {
        var statusCodes = new List<HttpStatusCode> { HttpStatusCode.OK, HttpStatusCode.Created };

        var baseStrategy = new RetryStrategyOptions<HttpResponseMessage>
        {
            Name = "BaseRetry",
            MaxRetryAttempts = 5,
            Delay = TimeSpan.FromMilliseconds(50),
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .HandleResult(response => response.StatusCode == HttpStatusCode.InternalServerError)
        };

        _retryStrategyRegistry.RegisterStrategy("BaseRetry", baseStrategy);

        var pipeline = _retryingHandler.GetRetryUntilStatusCodesResiliencePipeline(statusCodes, "BaseRetry");

        var executionCount = 0;

        async ValueTask<HttpResponseMessage> SimulatedHttpCall(CancellationToken token)
        {
            executionCount++;
            await Task.CompletedTask;
            return executionCount switch
            {
                1 => new HttpResponseMessage(HttpStatusCode.InternalServerError),
                2 => new HttpResponseMessage(HttpStatusCode.BadGateway),
                3 => new HttpResponseMessage(HttpStatusCode.OK),
                _ => new HttpResponseMessage(HttpStatusCode.OK)
            };
        }

        var response = await pipeline.ExecuteAsync(SimulatedHttpCall, default);

        Equal(HttpStatusCode.OK, response.StatusCode);
        Equal(3, executionCount);
    }
}
