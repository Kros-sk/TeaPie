namespace TeaPie.Templating;

internal sealed class TemplatingLimits
{
    public const int RegexTimeoutMilliseconds = 1000;

    public int MaxExpandedRequests { get; init; } = 1000;

    public int MaxRenderSteps { get; init; } = 200000;
}
