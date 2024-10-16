using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace TeaPie.ScriptHandling;

internal interface IScriptCompiler
{
    public (Script<object> script, Compilation compilation) CompileScript(string scriptContent);
}

internal class ScriptCompiler : IScriptCompiler
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

    public (Script<object> script, Compilation compilation) CompileScript(string scriptContent)
    {
        var scriptOptions = ScriptOptions.Default
            .AddReferences(AppDomain.CurrentDomain.GetAssemblies().Where(x => !string.IsNullOrEmpty(x.Location)))
            .WithImports(_defaultImports);

        var script = CSharpScript.Create(scriptContent, scriptOptions, typeof(Globals));
        var compilation = script.GetCompilation();

        return (script, compilation);
    }

    public class Globals
    {
#pragma warning disable IDE1006 // Naming Styles
        public TeaPie? tp { get; set; } = TeaPie.GetInstance(); //Public instance of context available for user
#pragma warning restore IDE1006
    }
}
