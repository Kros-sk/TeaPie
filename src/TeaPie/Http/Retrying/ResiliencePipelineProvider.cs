using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Net;
using System.Text;
using TeaPie.Http.Parsing;
using ResiliencePipeline = Polly.ResiliencePipeline<System.Net.Http.HttpResponseMessage>;
using RetryStrategy = Polly.Retry.RetryStrategyOptions<System.Net.Http.HttpResponseMessage>;

namespace TeaPie.Http.Retrying;

internal interface IResiliencePipelineProvider
{
    ResiliencePipeline GetResiliencePipeline(
        string nameOfBaseStrategy, RetryStrategy? explicitlyOverridenStrategy, IReadOnlyList<HttpStatusCode> statusCodes);
}

internal class ResiliencePipelineProvider(IRetryStrategyRegistry registry, ILogger<ResiliencePipelineProvider> logger)
    : IResiliencePipelineProvider
{
    private readonly IRetryStrategyRegistry _retryStrategyRegistry = registry;
    private readonly ILogger<ResiliencePipelineProvider> _logger = logger;

    public static readonly ResiliencePipeline DefaultResiliencePipeline =
        ResiliencePipeline.Empty;

    private readonly Dictionary<string, ResiliencePipeline> _resiliencePipelines =
        new() { { string.Empty, DefaultResiliencePipeline } };

    public ResiliencePipeline<HttpResponseMessage> GetResiliencePipeline(
        string nameOfBaseRetryStrategy, RetryStrategy? overridRetryStrategy, IReadOnlyList<HttpStatusCode> statusCodes)
    {
        var isBaseRetryStrategyAltered = false;
        CheckAndResolveBaseRetryStrategy(nameOfBaseRetryStrategy, out var finalRetryStrategy, out var nameOfFinalStrategy);
        ApplyExplicitOverridesIfAny(
            overridRetryStrategy, ref finalRetryStrategy, ref nameOfFinalStrategy, ref isBaseRetryStrategyAltered);
        ApplyRetryUntilStatusCodesConditionIfAny(
            statusCodes, ref finalRetryStrategy, ref nameOfFinalStrategy, ref isBaseRetryStrategyAltered);

        LogUsageOfRetryStrategy(nameOfFinalStrategy, finalRetryStrategy, isBaseRetryStrategyAltered);

        return GetResiliencePipeline(nameOfFinalStrategy, finalRetryStrategy, isBaseRetryStrategyAltered);
    }

    private void ApplyRetryUntilStatusCodesConditionIfAny(
        IReadOnlyList<HttpStatusCode> statusCodes,
        ref RetryStrategy finalRetryStrategy,
        ref string nameOfFinalStrategy,
        ref bool altered)
    {
        if (statusCodes.Any())
        {
            nameOfFinalStrategy = GetRetryUntilStatusCodesStrategyName(statusCodes, nameOfFinalStrategy);
            finalRetryStrategy = GetRetryStrategy(statusCodes, finalRetryStrategy);
            altered = true;
        }
    }

    private static void ApplyExplicitOverridesIfAny(
        RetryStrategy? overrideRetryStrategy,
        ref RetryStrategy finalRetryStrategy,
        ref string nameOfFinalStrategy,
        ref bool altered)
    {
        if (overrideRetryStrategy is not null)
        {
            finalRetryStrategy = MergeRetryStrategies(finalRetryStrategy, overrideRetryStrategy);
            nameOfFinalStrategy = finalRetryStrategy.Name!;
            altered = true;
        }
    }

    private void CheckAndResolveBaseRetryStrategy(
        string nameOfBaseStrategy,
        out RetryStrategy finalRetryStrategy,
        out string nameOfFinalStrategy)
    {
        if (!_retryStrategyRegistry.IsRegistered(nameOfBaseStrategy))
        {
            throw new InvalidOperationException($"Unable to find retry strategy with name '{nameOfBaseStrategy}'.");
        }

        finalRetryStrategy = _retryStrategyRegistry.Get(nameOfBaseStrategy);
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

            if (!_retryStrategyRegistry.IsRegistered(baseStrategyName))
            {
                _retryStrategyRegistry.Register(baseStrategyName, new() { Name = baseStrategyName });
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
        => RetryingDirectives.RetryStrategyDirectiveFullName + "-" + string.Join('-', statusCodes.Select(sc => (int)sc));

    private ResiliencePipeline<HttpResponseMessage> GetResiliencePipeline(string name, RetryStrategy retryStrategy, bool altered)
    {
        ResiliencePipeline<HttpResponseMessage>? pipeline;
        if (altered)
        {
            pipeline = BuildPipeline(retryStrategy);
        }
        else if (!_resiliencePipelines.TryGetValue(name, out pipeline))
        {
            pipeline = BuildPipeline(retryStrategy);

            _resiliencePipelines[name] = pipeline;
        }

        return pipeline;
    }

    private void LogUsageOfRetryStrategy(string nameOfFinalStrategy, RetryStrategy finalRetryStrategy, bool altered)
    {
        var isDefault = finalRetryStrategy.Name?.Equals(string.Empty) == true;

        _logger.LogDebug("Using{Altered}{Type} retry strategy with name '{Name}' for next request.",
            altered ? " altered" : string.Empty,
            isDefault ? " default" : string.Empty,
            isDefault ? string.Empty : nameOfFinalStrategy);
        _logger.LogDebug("Default retry strategy: {Description}", GetRetryStrategyDescription(finalRetryStrategy));
    }

    private static string? GetRetryStrategyDescription(RetryStrategy finalRetryStrategy)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine($"Name: {finalRetryStrategy.Name}");
        sb.AppendLine($"Maximal number of retry attempts: {finalRetryStrategy.MaxRetryAttempts}");
        sb.AppendLine($"Backoff type: '{finalRetryStrategy.BackoffType.ToString()}'");
        sb.AppendLine($"Delay: {finalRetryStrategy.Delay.ToString()}");
        sb.AppendLine($"Maximal delay: {finalRetryStrategy.MaxDelay?.ToString()}");
        sb.AppendLine($"Use jitter: {finalRetryStrategy.UseJitter}");

        return sb.ToString();
    }
}
