namespace TeaPie.StructureExploration.Records;

internal record File(string Path, string RelativePath, string Name, Folder ParentFolder);

internal record Folder(string Path, string RelativePath, string Name, Folder? ParentFolder = null);

internal record Script(File File);
