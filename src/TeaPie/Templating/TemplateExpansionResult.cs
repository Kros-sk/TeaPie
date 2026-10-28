namespace TeaPie.Templating;

internal sealed record TemplateExpansionResult(string Content, TemplatedRequestNames RequestNames);

internal sealed record TemplatedRequestNames(IReadOnlySet<string> Repeatable, IReadOnlySet<string> GeneratedByLoops)
{
    public static readonly TemplatedRequestNames None =
        new(new HashSet<string>(StringComparer.Ordinal), new HashSet<string>(StringComparer.Ordinal));
}
