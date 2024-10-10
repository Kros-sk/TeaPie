namespace TeaPie.StructureExploration;
internal class FolderNode(Folder data) : Node<Folder>(data)
{
    public List<FolderNode> FolderChildrenNodes { get; set; } = [];
    public List<TestCaseNode> FileChildrenNodes { get; set; } = [];
}
