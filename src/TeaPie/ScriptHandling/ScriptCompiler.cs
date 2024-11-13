using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Data;

namespace TeaPie.ScriptHandling;

internal interface IScriptCompiler
{
    Script<object> CompileScript(string scriptContent);
}

internal partial class ScriptCompiler(ILogger<ScriptCompiler> logger) : IScriptCompiler
{
    private readonly ILogger<ScriptCompiler> _logger = logger;

    public Script<object> CompileScript(string scriptContent)
    {
        var scriptOptions = ScriptOptions.Default
            .AddReferences(AppDomain.CurrentDomain.GetAssemblies().Where(x => !string.IsNullOrEmpty(x.Location)))
            .WithImports(Constants.DefaultImports);

        var script = CSharpScript.Create(scriptContent, scriptOptions, typeof(Globals));

        var compilationDiagnostics = script.Compile();
        ResolveCompilationDiagnostics(compilationDiagnostics);

        return script;
    }

    private void ResolveCompilationDiagnostics(ImmutableArray<Diagnostic> compilationDiagnostics)
    {
        var hasErrors = false;
        foreach (var diagnostic in compilationDiagnostics)
        {
            if (diagnostic.Severity == DiagnosticSeverity.Warning)
            {
                LogWarning(diagnostic.GetMessage());
            }
            else if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                if (!hasErrors)
                {
                    LogErrorsOccured(compilationDiagnostics.Count(x => x.Severity == DiagnosticSeverity.Error));
                    hasErrors = true;
                }

                LogError(diagnostic.GetMessage());
            }
        }

        if (hasErrors)
        {
            throw new SyntaxErrorException("Exception thrown during script compilation: Script contains syntax errors.");
        }
    }

    [LoggerMessage("Script has {count} syntax errors.", Level = LogLevel.Error)]
    partial void LogErrorsOccured(int count);

    [LoggerMessage("{warningMessage}", Level = LogLevel.Warning)]
    partial void LogWarning(string warningMessage);

    [LoggerMessage("{errorMessage}", Level = LogLevel.Error)]
    partial void LogError(string errorMessage);
}
