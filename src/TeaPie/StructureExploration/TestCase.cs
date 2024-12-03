namespace TeaPie.StructureExploration;

internal class TestCase(File requestFile)
{
    public IEnumerable<Script> PreRequestScripts = [];
    public File RequestsFile = requestFile;
    public IEnumerable<Script> PostResponseScripts = [];
}
