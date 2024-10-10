namespace TeaPie.StructureExploration;
internal class Node<T>(T data)
{
    public T Data { get; private set; } = data;
    public FolderNode? Parent { get; private set; }
}
