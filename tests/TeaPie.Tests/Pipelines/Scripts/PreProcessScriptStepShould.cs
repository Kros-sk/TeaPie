using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TeaPie.Pipelines.Application;
using TeaPie.Pipelines.Scripts;
using TeaPie.Scripts;
using TeaPie.Tests.Scripts;

namespace TeaPie.Tests.Pipelines.Scripts;

public class PreProcessScriptStepShould
{
    [Fact]
    public async void ScriptPreProcessorShouldReceiveCallWhenExecutingStep()
    {
        var logger = NullLogger.Instance;
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptWithSyntaxErrorPath);
        var accessor = new ScriptExecutionContextAccessor() { ScriptExecutionContext = context };
        context.RawContent = await File.ReadAllTextAsync(context.Script.File.Path);

        var processor = Substitute.For<IScriptPreProcessor>();

        var rootPath = ScriptIndex.RootSubFolderFullPath;
        var tempPath = Path.GetTempPath();

        var pipeline = new ApplicationPipeline();
        var appContext = new ApplicationContext(
            rootPath,
            logger,
            Substitute.For<IServiceProvider>(),
            tempPath);

        var step = new PreProcessScriptStep(pipeline, accessor, processor);
        await step.Execute(appContext);

        await processor.Received(1).ProcessScript(
            context.Script.File.Path,
            context.RawContent!,
            rootPath,
            tempPath,
            Arg.Any<List<string>>());
    }
}
