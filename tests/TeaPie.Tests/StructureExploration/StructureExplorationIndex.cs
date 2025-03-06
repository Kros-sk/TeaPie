namespace TeaPie.Tests.StructureExploration;

internal static class StructureExplorationIndex
{
    public const string RootFolderName = "Demo";

    public const string CollectionFolderName = "Structure";

    public static readonly string CollectionFolderRelativePath = Path.Combine(RootFolderName, CollectionFolderName);

    public static readonly string[] TestCasesRelativePaths = [
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
        Path.Combine(CollectionFolderName, $"ZeroLevelTest{Constants.RequestSuffix}{Constants.RequestFileExtension}")
    ];

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
        {TestCasesRelativePaths[8], (true, true)}
    };
}
