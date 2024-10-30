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
    public static IEnumerable<string> _defaultImports = [
        "System",
        "System.Collections.Generic",
        "System.IO",
        "System.Linq",
        "System.Net.Http",
        "System.Threading",
        "System.Threading.Tasks",
        "Microsoft.Extensions.Logging"
    ];

    private readonly ILogger<ScriptCompiler> _logger = logger;

    public Script<object> CompileScript(string scriptContent)
    {
        var scriptOptions = ScriptOptions.Default
            .AddReferences(AppDomain.CurrentDomain.GetAssemblies().Where(x => !string.IsNullOrEmpty(x.Location)))
            .WithImports(_defaultImports);

        var script = CSharpScript.Create(scriptContent, scriptOptions, typeof(Globals));

        var compilationDiagnostics = script.Compile();
        ResolveCompilationDiagnostics(compilationDiagnostics);

        return script;
    }

    private void ResolveCompilationDiagnostics(ImmutableArray<Diagnostic> compilationDiagnostics)
    {
        var errors = compilationDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
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
                    LogErrorsOccured(compilationDiagnostics.Length);
                }

                hasErrors = true;
                LogError(diagnostic.GetMessage());
            }
        }

        if (hasErrors)
        {
            throw new SyntaxErrorException("Exception thrown during compilation: Script contains syntax errors.");
        }
    }

    [LoggerMessage("Script has {count} syntax errors.", Level = LogLevel.Error)]
    partial void LogErrorsOccured(int count);

    [LoggerMessage("{warningMessage}", Level = LogLevel.Warning)]
    partial void LogWarning(string warningMessage);

    [LoggerMessage("{errorMessage}", Level = LogLevel.Error)]
    partial void LogError(string errorMessage);
}
