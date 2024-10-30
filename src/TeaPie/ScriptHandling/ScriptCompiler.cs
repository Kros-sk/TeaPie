using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;

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
        if (compilationDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
        {
            LogErrorsOccured(compilationDiagnostics.Length);

            foreach (var diagnostic in compilationDiagnostics)
            {
                LogError(diagnostic.GetMessage());
            }
            throw new InvalidOperationException("Exception during compilation: Script contains syntax errors.");
        }

        return script;
    }

    [LoggerMessage("Script has {count} syntax errors.", Level = LogLevel.Error)]
    partial void LogErrorsOccured(int count);

    [LoggerMessage("{errorMessage}", Level = LogLevel.Error)]
    partial void LogError(string errorMessage);
}
