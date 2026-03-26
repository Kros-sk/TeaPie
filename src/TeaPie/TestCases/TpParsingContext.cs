namespace TeaPie.TestCases;

internal class TpParsingContext(string content, string fallbackName)
{
    public string Content { get; } = content ?? throw new ArgumentNullException(nameof(content));
    public string FallbackName { get; } = !string.IsNullOrWhiteSpace(fallbackName)
        ? fallbackName
        : throw new ArgumentException("Value cannot be null or whitespace.", nameof(fallbackName));

    public List<TpTestCaseDefinition> Definitions { get; } = [];
}
