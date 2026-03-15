namespace TeaPie.TestCases;

internal class TpParsingContext
{
    private readonly List<TpTestCaseDefinition> _definitions = [];

    public required string Content { get; init; }
    public required string FallbackName { get; init; }
    public IReadOnlyList<TpTestCaseDefinition> Definitions => _definitions;

    internal void AddDefinition(TpTestCaseDefinition definition) => _definitions.Add(definition);
}
