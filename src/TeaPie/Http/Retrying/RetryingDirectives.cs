using TeaPie.Http.Parsing;

namespace TeaPie.Http.Retrying;

internal static class RetryingDirectives
{
    public const string RetryDirectivePrefix = "RETRY-";

    public const string RetryStrategyDirectiveName = "STRATEGY";
    public const string RetryStrategyDirectiveFullName = RetryDirectivePrefix + RetryStrategyDirectiveName;
    public const string RetryStrategyDirectiveParameterName = "StrategyName";
    public static readonly string RetryStrategySelectorDirectivePattern =
        HttpDirectivePatternBuilder.Create(RetryStrategyDirectiveName)
            .WithPrefix(RetryDirectivePrefix)
            .AddStringParameter(RetryStrategyDirectiveParameterName)
            .Build();

    public const string RetryUntilStatusCodesDirectiveName = "UNTIL-STATUS";
    public const string RetryUntilStatusCodesDirectiveFullName = RetryDirectivePrefix + RetryUntilStatusCodesDirectiveName;
    public const string RetryUntilStatusCodesDirectiveParameterName = "StatusCodes";
    public static readonly string RetryUntilStatusCodesDirectivePattern =
        HttpDirectivePatternBuilder.Create(RetryUntilStatusCodesDirectiveName)
            .WithPrefix(RetryDirectivePrefix)
            .AddStatusCodesParameter(RetryUntilStatusCodesDirectiveParameterName)
            .Build();

    public const string RetryMaxAttemptsDirectiveName = "MAX-ATTEMPTS";
    public const string RetryMaxAttemptsDirectiveFullName = RetryDirectivePrefix + RetryMaxAttemptsDirectiveName;
    public const string RetryMaxAttemptsDirectiveParameterName = "MaxAttempts";
    public static readonly string RetryMaxAttemptsDirectivePattern =
        HttpDirectivePatternBuilder.Create(RetryMaxAttemptsDirectiveName)
            .WithPrefix(RetryDirectivePrefix)
            .AddNumberParameter(RetryMaxAttemptsDirectiveParameterName)
            .Build();

    public const string RetryBackoffTypeDirectiveName = "BACKOFF-TYPE";
    public const string RetryBackoffTypeDirectiveFullName = RetryDirectivePrefix + RetryBackoffTypeDirectiveName;
    public const string RetryBackoffTypeDirectiveParameterName = "BackoffType";
    public static readonly string RetryBackoffTypeDirectivePattern =
        HttpDirectivePatternBuilder.Create(RetryBackoffTypeDirectiveName)
            .WithPrefix(RetryDirectivePrefix)
            .AddStringParameter(RetryBackoffTypeDirectiveParameterName)
            .Build();

    public const string RetryMaxDelayDirectiveName = "MAX-DELAY";
    public const string RetryMaxDelayDirectiveFullName = RetryDirectivePrefix + RetryMaxDelayDirectiveName;
    public const string RetryMaxDelayDirectiveParameterName = "MaxDelay";
    public static readonly string RetryMaxDelayDirectivePattern =
        HttpDirectivePatternBuilder.Create(RetryMaxDelayDirectiveName)
            .WithPrefix(RetryDirectivePrefix)
            .AddTimeOnlyParameter(RetryMaxDelayDirectiveParameterName)
            .Build();
}
