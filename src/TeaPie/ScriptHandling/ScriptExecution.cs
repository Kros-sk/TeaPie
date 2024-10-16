using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;

namespace TeaPie.ScriptHandling;

internal class ScriptExecution(StructureExploration.Script script)
{
    public StructureExploration.Script Script { get; set; } = script;
    internal string? RawContent { get; set; }
    internal string? ProcessedContent { get; set; }
    internal Script<object>? ScriptObject { get; set; }
    internal Compilation? Compilation { get; set; }
}
