using Microsoft.CodeAnalysis;

namespace TeaPie.StructureExploration;
internal class Script(File file)
{
    public File File { get; set; } = file;
    public string Content { get; set; } = string.Empty;
    public Microsoft.CodeAnalysis.Scripting.Script<object>? ScriptObject { get; set; } = null;
    public Compilation? Compilation { get; set; } = null;
}
