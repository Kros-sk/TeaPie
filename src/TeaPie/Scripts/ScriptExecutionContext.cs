using Microsoft.CodeAnalysis.Scripting;
using System.Diagnostics;
using Script = TeaPie.StructureExploration.IO.Script;

namespace TeaPie.Scripts;

[DebuggerDisplay("{Script}")]
internal class ScriptExecutionContext(Script script)
{
    public Script Script { get; set; } = script;
    public string? TemporaryPath { get; set; }
    public string? RawContent { get; set; }
    public string? ProcessedContent { get; set; }
    public Script<object>? ScriptObject { get; set; }
}
