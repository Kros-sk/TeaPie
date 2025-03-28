using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Scripts;
using TeaPie.StructureExploration;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.Tests.Scripts;

internal static class ScriptHelper
{
    public static ScriptExecutionContext GetScriptExecutionContext(string path)
    {
        var folder = new Folder(
            ScriptIndex.RootSubFolderFullPath,
            ScriptIndex.RootSubFolderRelativePath,
            ScriptIndex.RootFolderName,
            null);

        var file = InternalFile.Create(path, folder);

        var script = new Script(file);

        return new ScriptExecutionContext(script);
    }

    public static async Task ReadScript(ScriptExecutionContext context)
    {
        context.RawContent = await System.IO.File.ReadAllTextAsync(context.Script.File.Path);
    }

    public static async Task PreProccessScript(ScriptExecutionContext context)
    {
        var pathProvider = Substitute.For<IPathProvider>();
        var scriptLineResolversProvider = new ScriptLineResolversProvider(
            Substitute.For<INuGetPackageHandler>(),
            Substitute.For<IPathResolver>(),
            new TemporaryPathResolver(pathProvider, new RelativePathResolver()),
            Substitute.For<IExternalFileRegistry>(),
            pathProvider);

        var processor = new ScriptPreProcessor(scriptLineResolversProvider);

        var referencedScripts = new List<ScriptReference>();
        await processor.ProcessScript(context, referencedScripts);
    }

    public static async Task PrepareScriptForCompilation(ScriptExecutionContext context)
    {
        await ReadScript(context);
        await PreProccessScript(context);
    }

    private static void CompileScriptAndSaveMetadata(ScriptExecutionContext context)
    {
        var compiler = new ScriptCompiler(Substitute.For<ILogger<ScriptCompiler>>());
        context.ScriptObject = compiler.CompileScript(context.ProcessedContent!, context.Script.File.Path);
    }

    public static async Task PrepareScriptForExecution(ScriptExecutionContext context)
    {
        await PrepareScriptForCompilation(context);
        CompileScriptAndSaveMetadata(context);
    }
}
