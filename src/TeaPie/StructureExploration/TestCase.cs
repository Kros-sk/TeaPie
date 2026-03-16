using TeaPie.TestCases;

namespace TeaPie.StructureExploration;

internal class TestCase(InternalFile requestFile)
{
    public string Name = requestFile.Name
        .TrimSuffix(Constants.RequestSuffix + Constants.RequestFileExtension)
        .TrimSuffix(Constants.TestCaseFileExtension);
    public Folder ParentFolder = requestFile.ParentFolder;

    public IEnumerable<Script> PreRequestScripts = [];
    public InternalFile RequestsFile = requestFile;
    public IEnumerable<Script> PostResponseScripts = [];

    public bool IsFromTpFile { get; init; }
    public TpTestCaseDefinition? TpDefinition { get; init; }
}
