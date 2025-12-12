using Microsoft.Extensions.Logging;
using NSubstitute;
using Polly;
using Polly.Retry;
using System.Net;
using TeaPie.Http.Retrying;
using TeaPie.Testing;
using static Xunit.Assert;

namespace TeaPie.Tests.Http.Retrying;

public class ResiliencePipelineProviderShould
{
    private readonly IRetryStrategyRegistry _retryStrategyRegistry;
    private readonly ResiliencePipelineProvider _resiliencePipelineProvider;

    public ResiliencePipelineProviderShould()
    {
        _retryStrategyRegistry = new RetryStrategyRegistry();
        _resiliencePipelineProvider = new ResiliencePipelineProvider(
            _retryStrategyRegistry,
            Substitute.For<ILogger<ResiliencePipelineProvider>>(),
            Substitute.For<ITester>());
    }

    [Fact]
    public void ThrowExceptionWhenFetchingNonexistentResiliencePipeline()
    {
        var exception = Throws<InvalidOperationException>(()
            => _resiliencePipelineProvider.GetResiliencePipeline("NonexistentStrategy", null, []));

        Equal("Unable to find retry strategy with name 'NonexistentStrategy'.", exception.Message);
    }

    [Fact]
    public void ReturnExistingPipelineWhenFetchingResiliencePipelineByName()
    {
        var retryStrategy = new RetryStrategyOptions<HttpResponseMessage> { Name = "ExistingRetry" };
        _retryStrategyRegistry.Register("ExistingRetry", retryStrategy);

        var pipeline = _resiliencePipelineProvider.GetResiliencePipeline("ExistingRetry", null, []);

        NotNull(pipeline);
    }

    [Fact]
    public async Task RetryUntilMatchingStatusCodes()
    {
        var statusCodes = new List<HttpStatusCode> { HttpStatusCode.OK, HttpStatusCode.Created };
        var baseRetryStrategyName = string.Empty;
        var baseRetryStrategy = GetBaseRetryStrategy(ref baseRetryStrategyName);

        _retryStrategyRegistry.Register(baseRetryStrategyName, baseRetryStrategy);

        var pipeline = _resiliencePipelineProvider.GetResiliencePipeline(baseRetryStrategyName, null, statusCodes);

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
        var pipeline = _resiliencePipelineProvider.GetResiliencePipeline(string.Empty, null, statusCodes);

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

        var baseRetryStrategyName = string.Empty;
        var baseRetryStrategy = GetBaseRetryStrategy(ref baseRetryStrategyName);

        _retryStrategyRegistry.Register(baseRetryStrategyName, baseRetryStrategy);

        var pipeline = _resiliencePipelineProvider.GetResiliencePipeline(baseRetryStrategyName, null, statusCodes);

        var executionCount = 0;

        async ValueTask<HttpResponseMessage> SimulatedHttpCall(CancellationToken token)
        {
            executionCount++;
            await Task.CompletedTask;
            return new HttpResponseMessage(HttpStatusCode.NotFound);
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

        _retryStrategyRegistry.Register("BaseRetry", baseStrategy);

        var pipeline = _resiliencePipelineProvider.GetResiliencePipeline("BaseRetry", null, statusCodes);

        var executionCount = 0;

        async ValueTask<HttpResponseMessage> SimulatedHttpCall(CancellationToken token)
        {
            executionCount++;
            await Task.CompletedTask;
            return new HttpResponseMessage(HttpStatusCode.NotFound);
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

        _retryStrategyRegistry.Register("BaseRetry", baseStrategy);

        var pipeline = _resiliencePipelineProvider.GetResiliencePipeline("BaseRetry", null, statusCodes);

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

    [Theory]
    [MemberData(nameof(GetVariousCombinationsOfEntryParameters))]
    public async Task CreateResiliencePipelineCorrectlyFromMergedRetryStrategies(
        string baseStrategyName,
        RetryStrategyOptions<HttpResponseMessage>? explicitRetryStrategy,
        IReadOnlyList<HttpStatusCode> statusCodes,
        int expectedNumberOfExecutions,
        HttpStatusCode expectedStatusCode)
    {
        var baseStrategy = GetBaseRetryStrategy(ref baseStrategyName);

        _retryStrategyRegistry.Register("BaseRetry", baseStrategy);

        var pipeline = _resiliencePipelineProvider.GetResiliencePipeline(baseStrategyName, explicitRetryStrategy, statusCodes);

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

        Equal(expectedStatusCode, response.StatusCode);
        Equal(expectedNumberOfExecutions, executionCount);
    }

    private static RetryStrategyOptions<HttpResponseMessage> GetBaseRetryStrategy(ref string baseStrategyName)
    {
        RetryStrategyOptions<HttpResponseMessage> baseStrategy;
        if (baseStrategyName.Equals(string.Empty))
        {
            // Normally, this should be only default retry-strategy-options object, but since
            // Polly uses delay of 2 seconds... the tests execution takes around 30 seconds...
            baseStrategy = new RetryStrategyOptions<HttpResponseMessage>
            {
                Name = "BaseRetry",
                Delay = TimeSpan.FromMilliseconds(50)
            };

            baseStrategyName = baseStrategy.Name;
        }
        else
        {
            baseStrategy = new RetryStrategyOptions<HttpResponseMessage>
            {
                Name = "BaseRetry",
                MaxRetryAttempts = 5,
                Delay = TimeSpan.FromMilliseconds(50)
            };
        }

        return baseStrategy;
    }

    public static IEnumerable<object?[]> GetVariousCombinationsOfEntryParameters()
        =>
        [
            [
                string.Empty,
                null,
                new List<HttpStatusCode>(),
                1,
                HttpStatusCode.InternalServerError
            ],
            [
                string.Empty,
                null,
                new List<HttpStatusCode>() { HttpStatusCode.OK },
                3,
                HttpStatusCode.OK
            ],
            [
                string.Empty,
                new RetryStrategyOptions<HttpResponseMessage>() { MaxRetryAttempts = 1 },
                new List<HttpStatusCode>(),
                1,
                HttpStatusCode.InternalServerError
            ],
            [
                string.Empty,
                new RetryStrategyOptions<HttpResponseMessage>() { MaxDelay = TimeSpan.FromMilliseconds(100) },
                new List<HttpStatusCode>(),
                1,
                HttpStatusCode.InternalServerError
            ],
            [
                string.Empty,
                new RetryStrategyOptions<HttpResponseMessage>() { BackoffType = DelayBackoffType.Linear },
                new List<HttpStatusCode>(),
                1,
                HttpStatusCode.InternalServerError
            ],
            [
                string.Empty,
                new RetryStrategyOptions<HttpResponseMessage>() { MaxRetryAttempts = 1 },
                new List<HttpStatusCode>() { HttpStatusCode.OK },
                2,
                HttpStatusCode.BadGateway
            ],
            [
                string.Empty,
                new RetryStrategyOptions<HttpResponseMessage>() { MaxDelay = TimeSpan.FromMilliseconds(100) },
                new List<HttpStatusCode>() { HttpStatusCode.OK },
                3,
                HttpStatusCode.OK
            ],
            [
                string.Empty,
                new RetryStrategyOptions<HttpResponseMessage>() { BackoffType = DelayBackoffType.Linear },
                new List<HttpStatusCode>() { HttpStatusCode.OK },
                3,
                HttpStatusCode.OK
            ],
            [
                string.Empty,
                null,
                new List<HttpStatusCode>(),
                1,
                HttpStatusCode.InternalServerError
            ],
            [
                string.Empty,
                null,
                new List<HttpStatusCode>() { HttpStatusCode.OK },
                3,
                HttpStatusCode.OK
            ],
            [
                string.Empty,
                new RetryStrategyOptions<HttpResponseMessage>() { MaxRetryAttempts = 1 },
                new List<HttpStatusCode>(),
                1,
                HttpStatusCode.InternalServerError
            ],
            [
                string.Empty,
                new RetryStrategyOptions<HttpResponseMessage>() { MaxDelay = TimeSpan.FromMilliseconds(100) },
                new List<HttpStatusCode>(),
                1,
                HttpStatusCode.InternalServerError
            ],
            [
                string.Empty,
                new RetryStrategyOptions<HttpResponseMessage>() { BackoffType = DelayBackoffType.Linear },
                new List<HttpStatusCode>(),
                1,
                HttpStatusCode.InternalServerError
            ],
            [
                string.Empty,
                new RetryStrategyOptions<HttpResponseMessage>() { MaxRetryAttempts = 1 },
                new List<HttpStatusCode>() { HttpStatusCode.OK },
                2,
                HttpStatusCode.BadGateway
            ],
            [
                string.Empty,
                new RetryStrategyOptions<HttpResponseMessage>() { MaxDelay = TimeSpan.FromMilliseconds(100) },
                new List<HttpStatusCode>() { HttpStatusCode.OK },
                3,
                HttpStatusCode.OK
            ],
            [
                string.Empty,
                new RetryStrategyOptions<HttpResponseMessage>() { BackoffType = DelayBackoffType.Linear },
                new List<HttpStatusCode>() { HttpStatusCode.OK },
                3,
                HttpStatusCode.OK
            ],
            [
                "BaseRetry",
                null,
                new List<HttpStatusCode>(),
                1,
                HttpStatusCode.InternalServerError
            ],
            [
                "BaseRetry",
                null,
                new List<HttpStatusCode>() { HttpStatusCode.OK },
                3,
                HttpStatusCode.OK
            ],
            [
                "BaseRetry",
                new RetryStrategyOptions<HttpResponseMessage>() { MaxRetryAttempts = 1 },
                new List<HttpStatusCode>(),
                1,
                HttpStatusCode.InternalServerError
            ],
            [
                "BaseRetry",
                new RetryStrategyOptions<HttpResponseMessage>() { MaxDelay = TimeSpan.FromMilliseconds(100) },
                new List<HttpStatusCode>(),
                1,
                HttpStatusCode.InternalServerError
            ],
            [
                "BaseRetry",
                new RetryStrategyOptions<HttpResponseMessage>() { BackoffType = DelayBackoffType.Linear },
                new List<HttpStatusCode>(),
                1,
                HttpStatusCode.InternalServerError
            ],
            [
                "BaseRetry",
                new RetryStrategyOptions<HttpResponseMessage>() { MaxRetryAttempts = 1 },
                new List<HttpStatusCode>() { HttpStatusCode.OK },
                2,
                HttpStatusCode.BadGateway
            ],
            [
                "BaseRetry",
                new RetryStrategyOptions<HttpResponseMessage>() { MaxDelay = TimeSpan.FromMilliseconds(100) },
                new List<HttpStatusCode>() { HttpStatusCode.OK },
                3,
                HttpStatusCode.OK
            ],
            [
                "BaseRetry",
                new RetryStrategyOptions<HttpResponseMessage>() { BackoffType = DelayBackoffType.Linear },
                new List<HttpStatusCode>() { HttpStatusCode.OK },
                3,
                HttpStatusCode.OK
            ],
            [
                "BaseRetry",
                null,
                new List<HttpStatusCode>(),
                1,
                HttpStatusCode.InternalServerError
            ],
            [
                "BaseRetry",
                null,
                new List<HttpStatusCode>() { HttpStatusCode.OK },
                3,
                HttpStatusCode.OK
            ],
            [
                "BaseRetry",
                new RetryStrategyOptions<HttpResponseMessage>() { MaxRetryAttempts = 1 },
                new List<HttpStatusCode>(),
                1,
                HttpStatusCode.InternalServerError
            ],
            [
                "BaseRetry",
                new RetryStrategyOptions<HttpResponseMessage>() { MaxDelay = TimeSpan.FromMilliseconds(100) },
                new List<HttpStatusCode>(),
                1,
                HttpStatusCode.InternalServerError
            ],
            [
                "BaseRetry",
                new RetryStrategyOptions<HttpResponseMessage>() { BackoffType = DelayBackoffType.Linear },
                new List<HttpStatusCode>(),
                1,
                HttpStatusCode.InternalServerError
            ],
            [
                "BaseRetry",
                new RetryStrategyOptions<HttpResponseMessage>() { MaxRetryAttempts = 1 },
                new List<HttpStatusCode>() { HttpStatusCode.OK },
                2,
                HttpStatusCode.BadGateway
            ],
            [
                "BaseRetry",
                new RetryStrategyOptions<HttpResponseMessage>() { MaxDelay = TimeSpan.FromMilliseconds(100) },
                new List<HttpStatusCode>() { HttpStatusCode.OK },
                3,
                HttpStatusCode.OK
            ],
            [
                "BaseRetry",
                new RetryStrategyOptions<HttpResponseMessage>() { BackoffType = DelayBackoffType.Linear },
                new List<HttpStatusCode>() { HttpStatusCode.OK },
                3,
                HttpStatusCode.OK
            ],
        ];
}
