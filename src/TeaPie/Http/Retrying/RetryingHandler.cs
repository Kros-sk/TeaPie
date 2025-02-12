using Polly;
using Polly.Retry;
using System.Net;
using ResiliencePipeline = Polly.ResiliencePipeline<System.Net.Http.HttpResponseMessage>;
using RetryStrategy = Polly.Retry.RetryStrategyOptions<System.Net.Http.HttpResponseMessage>;

namespace TeaPie.Http.Retrying;

internal interface IRetryingHandler
{
    void RegisterRetryStrategy(RetryStrategy retryStrategy);

    ResiliencePipeline GetResiliencePipeline(string name);

    ResiliencePipeline GetResiliencePipeline(RetryStrategy retryStrategy);

    ResiliencePipeline GetRetryUntilStatusCodesResiliencePipeline(
        IList<HttpStatusCode> statusCodes, string nameOfRetryStrategy = "");
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

    private static void CheckName(RetryStrategy retryStrategy, out string name, string errorMessage)
        => name = retryStrategy.Name ?? throw new InvalidOperationException(errorMessage);

    public ResiliencePipeline GetResiliencePipeline(string name)
    {
        if (!_retryStrategiesRegistry.IsStrategyRegisterd(name))
        {
            throw new InvalidOperationException($"Unable to find retry strategy with name '{name}'.");
        }

        var retryStrategy = _retryStrategiesRegistry.GetStrategy(name);

        if (!_resiliencePipelines.TryGetValue(name, out var pipeline))
        {
            pipeline = BuildPipeline(retryStrategy);

            _resiliencePipelines[name] = pipeline;
        }

        return pipeline;
    }

    public ResiliencePipeline GetResiliencePipeline(RetryStrategy retryStrategy)
    {
        CheckName(retryStrategy, out var name, "Unable to get the resilience pipeline, if retry strategy doesn't have any name.");

        _retryStrategiesRegistry.RegisterStrategy(name, retryStrategy);

        var pipeline = BuildPipeline(retryStrategy);

        _resiliencePipelines[name] = pipeline;

        return pipeline;
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

            OnRetry = overwriteBy.OnRetry ?? toBeOverwritten.OnRetry,
        };

    private static ResiliencePipeline BuildPipeline(RetryStrategy retryStrategy)
        => new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(retryStrategy)
            .Build();

    public ResiliencePipeline GetRetryUntilStatusCodesResiliencePipeline(
        IList<HttpStatusCode> statusCodes,
        string baseRetryStrategyName = "")
    {
        var strategyName = GetStrategyName(statusCodes, baseRetryStrategyName);
        var retryStrategy = GetRetryStrategy(statusCodes, strategyName);

        return BuildPipeline(retryStrategy);
    }

    private RetryStrategy GetRetryStrategy(IList<HttpStatusCode> statusCodes, string strategyName)
    {
        var retryStrategy = _retryStrategiesRegistry.GetStrategy(strategyName);
        var retryStrategyWithCondition = new RetryStrategy()
        {
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .HandleResult(response => !statusCodes.Contains(response.StatusCode))
        };

        return MergeRetryStrategies(retryStrategy, retryStrategyWithCondition);
    }

    private string GetStrategyName(IList<HttpStatusCode> statusCodes, string baseStrategyName)
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

    private static string GetNameForRetryUntilStatusCodes(IList<HttpStatusCode> statusCodes)
        => HttpFileParserConstants.RetryStrategyDirectiveName + "-" + string.Join('-', statusCodes.Select(sc => (int)sc));
}
