namespace TeaPie.StructureExploration;

internal class TestCase(File requestFile)
{
    public string Name = requestFile.Name.TrimSuffix(Constants.RequestSuffix + Constants.RequestFileExtension);
    public Folder ParentFolder = requestFile.ParentFolder;

    public IEnumerable<Script> PreRequestScripts = [];
    public File RequestsFile = requestFile;
    public IEnumerable<Script> PostResponseScripts = [];
}
