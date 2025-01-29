using System.Diagnostics;

namespace TeaPie.StructureExploration;

[DebuggerDisplay("{RelativePath}")]
internal record File(string Path, string RelativePath, string Name, Folder ParentFolder)
{
    public static File Create(string filePath, Folder folder)
    {
        var fileName = System.IO.Path.GetFileName(filePath);
        return new File(
        filePath,
            $"{folder.RelativePath}{System.IO.Path.DirectorySeparatorChar}{fileName}",
            fileName,
            folder);
    }
}
