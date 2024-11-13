using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Extensions;
using TeaPie.ScriptHandling;
using TeaPie.StructureExploration.IO;
using File = TeaPie.StructureExploration.IO.File;

namespace TeaPie.Tests.ScriptHandling;

internal static class ScriptHelper
{
    public static ScriptExecutionContext GetScriptExecutionContext(string path)
    {
        var folder = new Folder(ScriptIndex.RootSubFolderPath, ScriptIndex.RootSubFolder, ScriptIndex.RootSubFolder, null);
        var file = new File(
            path,
            path.TrimRootPath(ScriptIndex.RootFolderPath),
            Path.GetFileName(path),
            folder);

        var script = new Script(file);

        return new ScriptExecutionContext(script);
    }

    public static async Task ReadScript(ScriptExecutionContext context)
    {
        context.RawContent = await System.IO.File.ReadAllTextAsync(context.Script.File.Path);
    }

    public static async Task PreProccessScript(ScriptExecutionContext context)
    {
        var nugetHandler = new NuGetPackageHandler(Substitute.For<ILogger<NuGetPackageHandler>>());
        var processor = new ScriptPreProcessor(nugetHandler, Substitute.For<ILogger<ScriptPreProcessor>>());
        var referencedScripts = new List<string>();
        context.ProcessedContent = await processor.ProcessScript(
            context.Script.File.Path,
            context.RawContent!,
            ScriptIndex.RootSubFolderPath,
            Path.GetTempPath(),
            referencedScripts);
    }

    public static async Task PrepareScriptForCompilation(ScriptExecutionContext context)
    {
        await ReadScript(context);
        await PreProccessScript(context);
    }

    private static void CompileScriptAndSaveMetadata(ScriptExecutionContext context)
    {
        var compiler = new ScriptCompiler(Substitute.For<ILogger<ScriptCompiler>>());
        context.ScriptObject = compiler.CompileScript(context.ProcessedContent!);
    }

    public static async Task PrepareScriptForExecution(ScriptExecutionContext context)
    {
        await PrepareScriptForCompilation(context);
        CompileScriptAndSaveMetadata(context);
    }
}
