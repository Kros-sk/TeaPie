using Polly;
using Polly.Retry;
using System.Net;
using TeaPie.Http.Parsing;
using ResiliencePipeline = Polly.ResiliencePipeline<System.Net.Http.HttpResponseMessage>;
using RetryStrategy = Polly.Retry.RetryStrategyOptions<System.Net.Http.HttpResponseMessage>;

namespace TeaPie.Http.Retrying;

internal interface IRetryingHandler
{
    void RegisterRetryStrategy(RetryStrategy retryStrategy);

    ResiliencePipeline GetResiliencePipeline(
        string nameOfBaseStrategy, RetryStrategy? explicitlyOverridenStrategy, IReadOnlyList<HttpStatusCode> statusCodes);
}

internal class RetryingHandler(IRetryStrategiesRegistry registry) : IRetryingHandler
{
    private readonly IRetryStrategiesRegistry _retryStrategiesRegistry = registry;

    public static ResiliencePipeline DefaultResiliencePipeline =
        ResiliencePipeline.Empty;

    private readonly Dictionary<string, ResiliencePipeline> _resiliencePipelines =
        new() { { string.Empty, DefaultResiliencePipeline } };

    public void RegisterRetryStrategy(RetryStrategy retryStrategy)
    {
        CheckName(retryStrategy, out var name, "Unable to register retry strategy with 'null' name.");
        _retryStrategiesRegistry.RegisterStrategy(name, retryStrategy);
    }

    public ResiliencePipeline<HttpResponseMessage> GetResiliencePipeline(
        string nameOfBaseRetryStrategy, RetryStrategy? overridRetryStrategy, IReadOnlyList<HttpStatusCode> statusCodes)
    {
        CheckAndResolveBaseRetryStrategy(nameOfBaseRetryStrategy, out var finalRetryStrategy, out var nameOfFinalStrategy);
        ApplyExplicitOverridesIfAny(overridRetryStrategy, ref finalRetryStrategy, ref nameOfFinalStrategy);
        ApplyRetryUntilStatusCodesConditionIfAny(statusCodes, ref finalRetryStrategy, ref nameOfFinalStrategy);

        return GetResiliencePipeline(nameOfFinalStrategy, finalRetryStrategy);
    }

    private void ApplyRetryUntilStatusCodesConditionIfAny(
        IReadOnlyList<HttpStatusCode> statusCodes,
        ref RetryStrategy finalRetryStrategy,
        ref string nameOfFinalStrategy)
    {
        if (statusCodes.Any())
        {
            nameOfFinalStrategy = GetRetryUntilStatusCodesStrategyName(statusCodes, nameOfFinalStrategy);
            finalRetryStrategy = GetRetryStrategy(statusCodes, finalRetryStrategy);
        }
    }

    private static void ApplyExplicitOverridesIfAny(
        RetryStrategy? overrideRetryStrategy,
        ref RetryStrategy finalRetryStrategy,
        ref string nameOfFinalStrategy)
    {
        if (overrideRetryStrategy is not null)
        {
            finalRetryStrategy = MergeRetryStrategies(finalRetryStrategy, overrideRetryStrategy);
            nameOfFinalStrategy = finalRetryStrategy.Name!;
        }
    }

    private void CheckAndResolveBaseRetryStrategy(
        string nameOfBaseStrategy,
        out RetryStrategy finalRetryStrategy,
        out string nameOfFinalStrategy)
    {
        if (!_retryStrategiesRegistry.IsStrategyRegisterd(nameOfBaseStrategy))
        {
            throw new InvalidOperationException($"Unable to find retry strategy with name '{nameOfBaseStrategy}'.");
        }

        finalRetryStrategy = _retryStrategiesRegistry.GetStrategy(nameOfBaseStrategy);
        nameOfFinalStrategy = nameOfBaseStrategy;
    }

    private static RetryStrategy MergeRetryStrategies(
        RetryStrategy toBeOverwritten,
        RetryStrategy overwriteBy)
        => new()
        {
            Name = overwriteBy.Name?.Equals(RetryingConstants.DefaultName) != true
                ? overwriteBy.Name
                : toBeOverwritten.Name,

            MaxRetryAttempts = overwriteBy.MaxRetryAttempts != RetryingConstants.DefaultRetryCount
                ? overwriteBy.MaxRetryAttempts
                : toBeOverwritten.MaxRetryAttempts,

            UseJitter = overwriteBy.UseJitter,

            Delay = overwriteBy.Delay != RetryingConstants.DefaultBaseDelay
                ? overwriteBy.Delay
                : toBeOverwritten.Delay,

            MaxDelay = overwriteBy.MaxDelay ?? toBeOverwritten.MaxDelay,

            DelayGenerator = overwriteBy.DelayGenerator ?? toBeOverwritten.DelayGenerator,

            BackoffType = overwriteBy.BackoffType != RetryingConstants.DefaultBackoffType
                ? overwriteBy.BackoffType
                : toBeOverwritten.BackoffType,

            ShouldHandle = AddNewRetryCondition(toBeOverwritten, overwriteBy),

            OnRetry = overwriteBy.OnRetry ?? toBeOverwritten.OnRetry
        };

    private static ResiliencePipeline BuildPipeline(RetryStrategy retryStrategy)
        => new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(retryStrategy)
            .Build();

    private static RetryStrategy GetRetryStrategy(IReadOnlyList<HttpStatusCode> statusCodes, RetryStrategy baseRetryStrategy)
    {
        var retryStrategyWithCondition = new RetryStrategy()
        {
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .HandleResult(response => !statusCodes.Contains(response.StatusCode))
        };

        return MergeRetryStrategies(baseRetryStrategy, retryStrategyWithCondition);
    }

    private string GetRetryUntilStatusCodesStrategyName(IReadOnlyList<HttpStatusCode> statusCodes, string baseStrategyName)
    {
        if (baseStrategyName.Equals(string.Empty))
        {
            baseStrategyName = GetNameForRetryUntilStatusCodes(statusCodes);

            if (!_retryStrategiesRegistry.IsStrategyRegisterd(baseStrategyName))
            {
                _retryStrategiesRegistry.RegisterStrategy(baseStrategyName, new() { Name = baseStrategyName });
            }
        }

        return baseStrategyName;
    }

    private static Func<RetryPredicateArguments<HttpResponseMessage>, ValueTask<bool>> AddNewRetryCondition(
        RetryStrategy retryStrategyWithSourceCondition,
        RetryStrategy retryStrategyWithNewCondition)
        => async args =>
        {
            var previousCondition = retryStrategyWithSourceCondition.ShouldHandle is not null &&
                await retryStrategyWithSourceCondition.ShouldHandle(args);

            var newCondition = retryStrategyWithNewCondition.ShouldHandle is not null &&
                await retryStrategyWithNewCondition.ShouldHandle(args);

            return previousCondition || newCondition;
        };

    private static string GetNameForRetryUntilStatusCodes(IReadOnlyList<HttpStatusCode> statusCodes)
        => HttpFileParserConstants.RetryStrategyDirectiveName + "-" + string.Join('-', statusCodes.Select(sc => (int)sc));

    private ResiliencePipeline<HttpResponseMessage> GetResiliencePipeline(string name, RetryStrategy retryStrategy)
    {
        if (!_resiliencePipelines.TryGetValue(name, out var pipeline))
        {
            pipeline = BuildPipeline(retryStrategy);

            _resiliencePipelines[name] = pipeline;
        }

        return pipeline;
    }

    private static void CheckName(RetryStrategy retryStrategy, out string name, string errorMessage)
        => name = retryStrategy.Name ?? throw new InvalidOperationException(errorMessage);
}
