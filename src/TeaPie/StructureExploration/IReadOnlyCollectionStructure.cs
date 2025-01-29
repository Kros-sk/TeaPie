using System.Diagnostics.CodeAnalysis;

namespace TeaPie.StructureExploration;

internal interface IReadOnlyCollectionStructure
{
    Folder? Root { get; }

    bool HasEnvironmentFile { get; }

    IReadOnlyCollection<Folder> Folders { get; }

    IReadOnlyCollection<TestCase> TestCases { get; }

    bool TryGetFolder(string path, [NotNullWhen(true)] out Folder? folder);

    bool TryGetTestCase(string path, [NotNullWhen(true)] out TestCase? testCase);
}
