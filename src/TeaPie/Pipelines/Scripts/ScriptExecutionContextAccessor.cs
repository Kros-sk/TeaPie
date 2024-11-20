using System.Diagnostics;
using TeaPie.Scripts;

namespace TeaPie.Pipelines.Scripts;

internal interface IScriptExecutionContextAccessor
{
    ScriptExecutionContext? ScriptExecutionContext { get; set; }
}

[DebuggerDisplay("{ScriptExecutionContext}")]
internal class ScriptExecutionContextAccessor : IScriptExecutionContextAccessor
{
    public ScriptExecutionContext? ScriptExecutionContext { get; set; }
}
