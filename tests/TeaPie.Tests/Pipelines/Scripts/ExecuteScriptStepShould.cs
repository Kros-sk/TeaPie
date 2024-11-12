using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TeaPie.Extensions;
using TeaPie.Pipelines.Application;
using TeaPie.Pipelines.Scripts;
using TeaPie.ScriptHandling;
using TeaPie.StructureExploration.IO;
using TeaPie.Tests.ScriptHandling;
using File = TeaPie.StructureExploration.IO.File;

namespace TeaPie.Tests.Pipelines.Scripts;
public class ExecuteScriptStepShould
{
    [Fact]
    public async void ScriptShouldAccessTeaPieInstanceWithoutAnyProblem()
    {
        var logger = NullLogger.Instance;
        var context = GetScriptExecutionContext(ScriptIndex.ScriptAccessingTeaPieInstance);
        var accessor = new ScriptExecutionContextAccessor() { ScriptExecutionContext = context };
        TeaPie.Create(logger);
        await PrepareScriptForExecution(context);

        var step = new ExecuteScriptStep(accessor);
        var appContext = new ApplicationContext(string.Empty, logger, Substitute.For<IServiceProvider>());

        await step.Execute(appContext);
    }

    [Fact]
    public async void ScriptWithNuGetPackageShouldExecuteWithoutAnyProblem()
    {
        var logger = NullLogger.Instance;
        var context = GetScriptExecutionContext(ScriptIndex.ScriptWithOneNuGetDirectivePath);
        var accessor = new ScriptExecutionContextAccessor() { ScriptExecutionContext = context };
        await PrepareScriptForExecution(context);

        var step = new ExecuteScriptStep(accessor);
        var appContext = new ApplicationContext(string.Empty, logger, Substitute.For<IServiceProvider>());

        await step.Execute(appContext);
    }

    private static async Task PrepareScriptForExecution(ScriptExecutionContext context)
    {
        context.RawContent = await System.IO.File.ReadAllTextAsync(context.Script.File.Path);
        await PreProccessScript(context);
        CompileScriptAndSaveMetadata(context);
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

    private static void CompileScriptAndSaveMetadata(ScriptExecutionContext context)
    {
        var compiler = new ScriptCompiler(Substitute.For<ILogger<ScriptCompiler>>());
        context.ScriptObject = compiler.CompileScript(context.ProcessedContent!);
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
