﻿using Microsoft.CodeAnalysis.Scripting;
using System.Diagnostics;
using Script = TeaPie.StructureExploration.Script;

namespace TeaPie.Scripts;

[DebuggerDisplay("{Script}")]
internal class ScriptExecutionContext(Script script) : IDisposable
{
    public Script Script { get; set; } = script;
    public string? RawContent { get; set; }
    public string? ProcessedContent { get; set; }
    public Script<object>? ScriptObject { get; set; }

    public void Dispose()
    {
        RawContent = null;
        ProcessedContent = null;
        ScriptObject = null;
    }
}
