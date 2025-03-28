namespace TeaPie.StructureExploration;

internal class TestCase(InternalFile requestFile)
{
    public string Name = requestFile.Name.TrimSuffix(Constants.RequestSuffix + Constants.RequestFileExtension);
    public Folder ParentFolder = requestFile.ParentFolder;

    public IEnumerable<Script> PreRequestScripts = [];
    public InternalFile RequestsFile = requestFile;
    public IEnumerable<Script> PostResponseScripts = [];
}
