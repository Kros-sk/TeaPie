namespace TeaPie.StructureExploration;

internal interface IStructureExplorer
{
    public Dictionary<string, TestCase> ExploreFileSystem(string rootPath);
}
