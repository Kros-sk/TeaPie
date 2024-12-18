using System.Diagnostics;

namespace TeaPie.Scripts;

internal interface IScriptExecutionContextAccessor : IContextAccessor<ScriptExecutionContext>;

[DebuggerDisplay("{Context}")]
internal class ScriptExecutionContextAccessor : IScriptExecutionContextAccessor
{
    public ScriptExecutionContext? Context { get; set; }
}
