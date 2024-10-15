namespace TeaPie.StructureExploration;

internal class File(string path, string relativePath, string name, Folder folder)
{
    public string Path { get; set; } = path;
    public string RelativePath { get; set; } = relativePath;
    public string Name { get; set; } = name;
    public Folder ParentFolder { get; set; } = folder;
}
