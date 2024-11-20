using System.Diagnostics;

namespace TeaPie.StructureExploration.IO;

[DebuggerDisplay("{RelativePath}")]
internal record File(string Path, string RelativePath, string Name, Folder ParentFolder);