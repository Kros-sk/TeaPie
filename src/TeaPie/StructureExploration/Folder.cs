
namespace TeaPie.StructureExploration;

internal record Folder(string Path, string RelativePath, string Name, Folder? ParentFolder = null);