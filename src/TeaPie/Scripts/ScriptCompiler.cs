using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Data;

namespace TeaPie.Scripts;

internal interface IScriptCompiler
{
    Script<object> CompileScript(string scriptContent, string path);
}

internal partial class ScriptCompiler(ILogger<ScriptCompiler> logger) : IScriptCompiler
{
    private readonly ILogger<ScriptCompiler> _logger = logger;

    public Script<object> CompileScript(string scriptContent, string path)
    {
        var scriptOptions = ScriptOptions.Default
            .AddReferences(AppDomain.CurrentDomain.GetAssemblies().Where(x => !string.IsNullOrEmpty(x.Location)))
            .WithImports(ScriptsConstants.DefaultImports);

        var script = CSharpScript.Create(scriptContent, scriptOptions, typeof(Globals));

        var compilationDiagnostics = script.Compile();
        ResolveCompilationDiagnostics(compilationDiagnostics, path);

        return script;
    }

    private void ResolveCompilationDiagnostics(ImmutableArray<Diagnostic> compilationDiagnostics, string path)
    {
        var filteredDiagnostics = compilationDiagnostics
            .Where(d => !ScriptsConstants.SuppressedWarnings.Contains(d.Id))
            .GroupBy(d => d.Severity);

        foreach (var group in filteredDiagnostics)
        {
            if (group.Key == DiagnosticSeverity.Warning)
            {
                LogDiagnostics(path, group, LogWarningsOccured, LogWarning);
            }
            else if (group.Key == DiagnosticSeverity.Error)
            {
                LogDiagnostics(path, group, LogErrorsOccured, LogError);
                throw new SyntaxErrorException("Exception thrown during script compilation: Script contains syntax errors.");
            }
        }
    }

    private static void LogDiagnostics(
        string path,
        IGrouping<DiagnosticSeverity, Diagnostic> diagnostics,
        Action<string, int> logGroupExistence,
        Action<string> logSingleOccurence)
    {
        logGroupExistence(path, diagnostics.Count());
        foreach (var diagnostic in diagnostics)
        {
            logSingleOccurence(diagnostic.GetMessage());
        }
    }

    [LoggerMessage("Script on path '{path}' has {count} warnings.", Level = LogLevel.Warning)]
    partial void LogWarningsOccured(string path, int count);

    [LoggerMessage("Script on path '{path}' has {count} syntax errors.", Level = LogLevel.Error)]
    partial void LogErrorsOccured(string path, int count);

    [LoggerMessage("Script warning: {warningMessage}", Level = LogLevel.Debug)]
    partial void LogWarning(string warningMessage);

    [LoggerMessage("{errorMessage}", Level = LogLevel.Error)]
    partial void LogError(string errorMessage);
}
