namespace TeaPie.StructureExploration;

internal interface IReadOnlyCollectionStructure
{
    Folder? Root { get; }

    bool HasEnvironmentFile { get; }

    IReadOnlyCollection<Folder> Folders { get; }

    IReadOnlyCollection<TestCase> TestCases { get; }
}
