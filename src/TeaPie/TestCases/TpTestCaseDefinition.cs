namespace TeaPie.TestCases;

internal record TpTestCaseDefinition(
    string Name,
    string? InitContent,
    string HttpContent,
    string? TestContent);
