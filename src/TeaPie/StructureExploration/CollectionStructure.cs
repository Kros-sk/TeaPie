namespace TeaPie.StructureExploration;

internal class CollectionStructure : IReadOnlyCollectionStructure
{
    private readonly Dictionary<string, Folder> _folders = [];
    private readonly Dictionary<string, TestCase> _testCases = [];

    public Folder? Root { get; }

    public CollectionStructure() { }

    public CollectionStructure(Folder root)
    {
        Root = root;
        _folders.Add(root.Path, root);
    }

    public IReadOnlyCollection<Folder> Folders => _folders.Values;
    public IReadOnlyCollection<TestCase> TestCases => _testCases.Values;

    public bool TryAddFolder(Folder folder) => _folders.TryAdd(folder.Path, folder);

    /// <summary>
    /// Try to add given test-case to the structure. If parent folder is not in the structure yet,
    /// then attempt to add folder is done.
    /// </summary>
    /// <param name="testCase">Test-case to be added to the structure.</param>
    public bool TryAddTestCase(TestCase testCase)
    {
        if (!_folders.ContainsKey(testCase.ParentFolder.RelativePath))
        {
            TryAddFolder(testCase.ParentFolder);
        }

        return _testCases.TryAdd(testCase.RequestsFile.Path, testCase);
    }
}
