namespace TeaPie.StructureExploration;

/// <summary>
/// File which is located outside of the collection structure.
/// </summary>
/// <param name="Path">Path to file.</param>
internal record ExternalFile(string Path) : File(Path)
{
    public string TempPath { get; set; } = string.Empty;
}
