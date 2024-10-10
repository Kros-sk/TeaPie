namespace TeaPie.StructureExploration;
internal interface IFileSystemEntity
{
    public string Path { get; set; }
    public string RelativePath { get; set; }
    public string Name { get; set; }
}
