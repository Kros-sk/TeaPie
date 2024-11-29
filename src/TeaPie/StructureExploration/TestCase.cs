namespace TeaPie.StructureExploration;

internal class TestCase(File requestFile)
{
    public IEnumerable<Script> PreRequestScripts = [];
    public File Request = requestFile;
    public IEnumerable<Script> PostResponseScripts = [];
}
