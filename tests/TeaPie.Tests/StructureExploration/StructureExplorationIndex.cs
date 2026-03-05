namespace TeaPie.Tests.StructureExploration;

internal static class StructureExplorationIndex
{
    public const string RootFolderName = "Demo";

    public const string CollectionFolderName = "Structure";

    public static readonly string CollectionFolderRelativePath = Path.Combine(RootFolderName, CollectionFolderName);

    public static readonly string[] TestCasesRelativePaths = [
        // .http test cases (depth-first, alpha order within each folder)
        Path.Combine(CollectionFolderName, "FirstFolder", "FirstFolderInFirstFolder", $"Seed{Constants.RequestFileExtension}"),
        Path.Combine(CollectionFolderName, "FirstFolder", "FirstFolderInFirstFolder",
            $"Test1.1.1{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine(CollectionFolderName, "FirstFolder", "SecondFolderInFirstFolder", "FFinSFinFF",
            $"Test1.2.1.1{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine(CollectionFolderName, "FirstFolder", "SecondFolderInFirstFolder",
            $"Test1.2.1{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine(CollectionFolderName, "FirstFolder", "SecondFolderInFirstFolder",
            $"Test1.2.2{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine(CollectionFolderName, "SecondFolder", "FirstFolderInSecondFolder",
            $"ATest{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine(CollectionFolderName, $"AZeroLevelTest{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine(CollectionFolderName, $"TheZeroLevelTest{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        Path.Combine(CollectionFolderName, $"ZeroLevelTest{Constants.RequestSuffix}{Constants.RequestFileExtension}"),
        // .tp test cases (discovered after .http files in same folder, alpha order by file name)
        // HttpOnlyTpTest.tp → 1 test case
        Path.Combine(CollectionFolderName, $"HttpOnlyTpTest{Constants.TestCaseFileExtension}"),
        // MultiTpTest.tp → 2 test cases (same file path for both)
        Path.Combine(CollectionFolderName, $"MultiTpTest{Constants.TestCaseFileExtension}"),
        Path.Combine(CollectionFolderName, $"MultiTpTest{Constants.TestCaseFileExtension}"),
        // SingleTpTest.tp → 1 test case
        Path.Combine(CollectionFolderName, $"SingleTpTest{Constants.TestCaseFileExtension}")
    ];

    // Maps relative path → (hasPreRequest, hasPostResponse) at exploration time.
    // Note: .tp test cases have empty scripts at exploration time (populated at runtime via TpDefinition).
    public static readonly Dictionary<string, (bool hasPreRequest, bool hasPostResponse)> TestCasesScriptsMap = new()
    {
        {TestCasesRelativePaths[0], (false, false)},
        {TestCasesRelativePaths[1], (true, false)},
        {TestCasesRelativePaths[2], (true, false)},
        {TestCasesRelativePaths[3], (false, false)},
        {TestCasesRelativePaths[4], (false, false)},
        {TestCasesRelativePaths[5], (false, true)},
        {TestCasesRelativePaths[6], (true, true)},
        {TestCasesRelativePaths[7], (true, true)},
        {TestCasesRelativePaths[8], (true, true)},
        // .tp test cases: scripts are populated at runtime, not at exploration time
        {TestCasesRelativePaths[9], (false, false)},
        // MultiTpTest.tp paths are the same - both map to (false, false)
        // {TestCasesRelativePaths[10], (false, false)} - duplicate key, handled in GetExpectedScripts helper
        // {TestCasesRelativePaths[12], (false, false)} - same
        {TestCasesRelativePaths[12], (false, false)}
    };

    public static (bool hasPreRequest, bool hasPostResponse) GetExpectedScripts(string relativePath)
    {
        if (TestCasesScriptsMap.TryGetValue(relativePath, out var result))
        {
            return result;
        }

        // .tp test cases (MultiTpTest.tp duplicates) have empty scripts at exploration time
        if (relativePath.EndsWith(Constants.TestCaseFileExtension, StringComparison.OrdinalIgnoreCase))
        {
            return (false, false);
        }

        throw new KeyNotFoundException($"No expected scripts entry for path: {relativePath}");
    }
}
