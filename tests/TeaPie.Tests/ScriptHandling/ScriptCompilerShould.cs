using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TeaPie.Extensions;
using TeaPie.Pipelines.Scripts;
using TeaPie.ScriptHandling;
using TeaPie.StructureExploration.IO;
using File = TeaPie.StructureExploration.IO.File;

namespace TeaPie.Tests.ScriptHandling;

public class ScriptCompilerShould
{
    [Fact]
    public async void ScriptWithSyntaxErrorShouldThrowException()
    {
        var logger = NullLogger.Instance;
        var context = GetScriptExecutionContext(ScriptIndex.ScriptWithSyntaxErrorPath);
        var accessor = new ScriptExecutionContextAccessor() { ScriptExecutionContext = context };
        await PrepareScriptForCompilation(context);

        var compiler = new ScriptCompiler(Substitute.For<ILogger<ScriptCompiler>>());

        compiler.Invoking(c => c.CompileScript(context.ProcessedContent!)).Should().Throw<InvalidOperationException>();
    }

    private static async Task PrepareScriptForCompilation(ScriptExecutionContext context)
    {
        context.RawContent = await System.IO.File.ReadAllTextAsync(context.Script.File.Path);
        await PreProccessScript(context);
    }

    private static async Task PreProccessScript(ScriptExecutionContext context)
    {
        var nugetHandler = new NugetPackageHandler(Substitute.For<ILogger<NugetPackageHandler>>());
        var processor = new ScriptPreProcessor(nugetHandler, Substitute.For<ILogger<ScriptPreProcessor>>());
        var referencedScripts = new List<string>();
        context.ProcessedContent = await processor.ProcessScript(
            context.Script.File.Path,
            context.RawContent!,
            ScriptIndex.RootSubFolderPath,
            Path.GetTempPath(),
            referencedScripts);
    }

    private static ScriptExecutionContext GetScriptExecutionContext(string path)
    {
        var folder = new Folder(ScriptIndex.RootSubFolderPath, ScriptIndex.RootSubFolder, ScriptIndex.RootSubFolder, null);
        var file = new File(
            path,
            path.TrimRootPath(ScriptIndex.RootFolderPath),
            Path.GetFileName(ScriptIndex.ScriptAccessingTeaPieInstance),
            folder);

        var script = new Script(file);

        return new ScriptExecutionContext(script);
    }
}
