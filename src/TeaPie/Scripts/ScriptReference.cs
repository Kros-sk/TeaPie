namespace TeaPie.Scripts;

internal record ScriptReference(string RealPath, string TempPath, bool IsExternal = false);
