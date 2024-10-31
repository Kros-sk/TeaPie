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

public class PreProcessScriptStepShould
{
    [Fact]
    public async void ScriptPreProcessorShouldReceiveCallWhenExecutingStep()
    {
        var logger = NullLogger.Instance;
        var context = GetScriptExecutionContext(ScriptIndex.ScriptWithSyntaxErrorPath);
        var accessor = new ScriptExecutionContextAccessor() { ScriptExecutionContext = context };
        context.RawContent = await System.IO.File.ReadAllTextAsync(context.Script.File.Path);

        var processor = Substitute.For<IScriptPreProcessor>();

        var rootPath = ScriptIndex.RootSubFolderPath;
        var tempPath = Path.GetTempPath();

        var pipeline = new ApplicationPipeline();
        var appContext = new ApplicationContext(rootPath, logger, Substitute.For<IServiceProvider>(), tempPath);

        var step = new PreProcessScriptStep(pipeline, accessor, processor);
        await step.Execute(appContext);

        await processor.Received(1).ProcessScript(
            context.Script.File.Path,
            context.RawContent!,
            rootPath,
            tempPath,
            Arg.Any<List<string>>());
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
