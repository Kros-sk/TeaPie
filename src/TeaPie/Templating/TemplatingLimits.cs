namespace TeaPie.Templating;

internal sealed class TemplatingLimits
{
    public int MaxExpandedRequests { get; init; } = 1000;

    public int MaxRenderSteps { get; init; } = 200000;
}
