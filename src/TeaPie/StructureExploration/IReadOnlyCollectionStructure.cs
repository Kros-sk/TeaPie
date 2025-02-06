using System.Diagnostics.CodeAnalysis;

namespace TeaPie.StructureExploration;

internal interface IReadOnlyCollectionStructure
{
    Folder? Root { get; }

    File? EnvironmentFile { get; }

    [MemberNotNullWhen(true, nameof(EnvironmentFile))]
    bool HasEnvironmentFile { get; }

    Script? InitializationScript { get; }

    [MemberNotNullWhen(true, nameof(InitializationScript))]
    bool HasInitializationScript { get; }

    IReadOnlyCollection<Folder> Folders { get; }

    IReadOnlyCollection<TestCase> TestCases { get; }

    bool TryGetFolder(string path, [NotNullWhen(true)] out Folder? folder);

    bool TryGetTestCase(string path, [NotNullWhen(true)] out TestCase? testCase);
}
