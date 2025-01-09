namespace TeaPie.StructureExploration;

internal interface IReadOnlyCollectionStructure
{
    Folder? Root { get; }

    IReadOnlyCollection<Folder> Folders { get; }

    IReadOnlyCollection<TestCase> TestCases { get; }
}
