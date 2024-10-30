namespace TeaPie.StructureExploration.IO;

internal record Folder(string Path, string RelativePath, string Name, Folder? ParentFolder = null);