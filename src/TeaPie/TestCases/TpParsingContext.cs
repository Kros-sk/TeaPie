namespace TeaPie.TestCases;

internal class TpParsingContext
{
    public required string Content { get; init; }
    public required string FallbackName { get; init; }
    public List<TpTestCaseDefinition> Definitions { get; } = [];
}
