using System.Diagnostics;

namespace TeaPie.Scripts;

internal interface IScriptExecutionContextAccessor
{
    ScriptExecutionContext? ScriptExecutionContext { get; set; }
}

[DebuggerDisplay("{ScriptExecutionContext}")]
internal class ScriptExecutionContextAccessor : IScriptExecutionContextAccessor
{
    public ScriptExecutionContext? ScriptExecutionContext { get; set; }
}
