using System.Diagnostics;

namespace TeaPie.StructureExploration;

/// <summary>
/// File within collection structure.
/// </summary>
/// <param name="Path">Path to file.</param>
/// <param name="RelativePath">Path relative to collection folder.</param>
/// <param name="ParentFolder">Folder to which this file belongs.</param>
[DebuggerDisplay("{RelativePath}")]
internal record InternalFile(string Path, string RelativePath, Folder ParentFolder) : File(Path, RelativePath)
{
    public static InternalFile Create(string filePath, Folder folder)
    {
        var fileName = System.IO.Path.GetFileName(filePath);
        return new InternalFile(
            filePath,
            System.IO.Path.Combine(folder.RelativePath, fileName),
            folder);
    }
}
