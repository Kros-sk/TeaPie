using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Data;
using TeaPie.Extensions;
using TeaPie.Pipelines.Application;
using TeaPie.Pipelines.Scripts;
using TeaPie.ScriptHandling;
using TeaPie.StructureExploration.IO;
using File = TeaPie.StructureExploration.IO.File;

namespace TeaPie.Tests.ScriptHandling;

public class CompileScriptStepShould
{
    [Fact]
    public async void CompilerShouldReceiveCallToCompileTheScript()
    {
        var logger = NullLogger.Instance;
        var context = GetScriptExecutionContext(ScriptIndex.PlainScriptPath);
        var accessor = new ScriptExecutionContextAccessor() { ScriptExecutionContext = context };
        await PrepareScriptForCompilation(context);

        var compiler = Substitute.For<IScriptCompiler>();

        var appContext = new ApplicationContext(string.Empty, logger, Substitute.For<IServiceProvider>());
        var step = new CompileScriptStep(accessor, compiler);

        await step.Execute(appContext);

        compiler.Received(1).CompileScript(context.ProcessedContent!);
    }

    [Fact]
    public async void ScriptWithSyntaxErrorShouldThrowProperException()
    {
        var logger = NullLogger.Instance;
        var context = GetScriptExecutionContext(ScriptIndex.ScriptWithSyntaxErrorPath);
        var accessor = new ScriptExecutionContextAccessor() { ScriptExecutionContext = context };
        await PrepareScriptForCompilation(context);

        var compiler = new ScriptCompiler(Substitute.For<ILogger<ScriptCompiler>>());

        var appContext = new ApplicationContext(string.Empty, logger, Substitute.For<IServiceProvider>());
        var step = new CompileScriptStep(accessor, compiler);

        await step.Invoking(async step => await step.Execute(appContext)).Should().ThrowAsync<SyntaxErrorException>();
    }

    private static async Task PrepareScriptForCompilation(ScriptExecutionContext context)
    {
        context.RawContent = await System.IO.File.ReadAllTextAsync(context.Script.File.Path);
        await PreProccessScript(context);
    }

    private static async Task PreProccessScript(ScriptExecutionContext context)
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
