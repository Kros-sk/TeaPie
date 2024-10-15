namespace TeaPie.StructureExploration;

internal class Folder(string path, string relativePath, string name, Folder? parentFolder = null)
{
    public string Path { get; set; } = path;
    public string RelativePath { get; set; } = relativePath;
    public string Name { get; set; } = name;
    public Folder? ParentFolder { get; set; } = parentFolder;
}
