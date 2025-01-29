using System.Diagnostics.CodeAnalysis;

namespace TeaPie.StructureExploration;

internal class CollectionStructure : IReadOnlyCollectionStructure
{
    public CollectionStructure() { }

    public CollectionStructure(Folder root)
    {
        Root = root;
        _folders.Add(root.Path, root);
    }

    #region Folders, Test-Cases
    private readonly Dictionary<string, Folder> _folders = [];
    private readonly Dictionary<string, TestCase> _testCases = [];

    public Folder? Root { get; }

    public IReadOnlyCollection<Folder> Folders => _folders.Values;
    public IReadOnlyCollection<TestCase> TestCases => _testCases.Values;

    public bool TryGetFolder(string path, [NotNullWhen(true)] out Folder? folder)
        => _folders.TryGetValue(path, out folder);

    public bool TryGetTestCase(string path, [NotNullWhen(true)] out TestCase? testCase)
        => _testCases.TryGetValue(path, out testCase);

    public bool TryAddFolder(Folder folder) => _folders.TryAdd(folder.Path, folder);

    /// <summary>
    /// Try to add given test-case to the structure. If parent folder is not in the structure yet,
    /// then attempt to add folder is done.
    /// </summary>
    /// <param name="testCase">Test-case to be added to the structure.</param>
    public bool TryAddTestCase(TestCase testCase)
    {
        if (!_folders.ContainsKey(testCase.ParentFolder.Path))
        {
            TryAddFolder(testCase.ParentFolder);
        }

        return _testCases.TryAdd(testCase.RequestsFile.Path, testCase);
    }
    #endregion

    #region Environment File
    public File? EnvironmentFile { get; private set; }

    [MemberNotNullWhen(true, nameof(EnvironmentFile))]
    public bool HasEnvironmentFile => EnvironmentFile != null;

    [MemberNotNull(nameof(EnvironmentFile))]
    internal void SetEnvironmentFile(File? file)
    {
        if (file is null)
        {
            throw new InvalidOperationException("Unable to set environment file to null.");
        }

        EnvironmentFile = file;
    }
    #endregion
}
