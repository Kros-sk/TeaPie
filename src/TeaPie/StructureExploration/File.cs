using System.Diagnostics;

namespace TeaPie.StructureExploration;

[DebuggerDisplay("{GetDisplayPath()}")]
internal record File(string Path, string RelativePath = "")
{
    public string Name { get; } = System.IO.Path.GetFileName(Path);

    public string GetDisplayPath() => string.IsNullOrEmpty(RelativePath) ? Path : RelativePath;

    public static bool BelongsTo(string filePath, string rootPath)
        => filePath.Trim().StartsWith(rootPath.Trim());
}
