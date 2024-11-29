using System.Diagnostics;

namespace TeaPie.StructureExploration;

[DebuggerDisplay("{RelativePath}")]
internal record File(string Path, string RelativePath, string Name, Folder ParentFolder);
