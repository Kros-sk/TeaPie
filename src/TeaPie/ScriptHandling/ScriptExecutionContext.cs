using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;

namespace TeaPie.ScriptHandling;

internal class ScriptExecutionContext(StructureExploration.Script script)
{
    public StructureExploration.Script Script { get; set; } = script;
    public string? TemporaryPath { get; set; }
    public string? RawContent { get; set; }
    public string? ProcessedContent { get; set; }
    public Script<object>? ScriptObject { get; set; }
    public Compilation? Compilation { get; set; }
}
