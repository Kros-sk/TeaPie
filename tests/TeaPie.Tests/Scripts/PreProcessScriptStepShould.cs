using NSubstitute;
using TeaPie.Scripts;

namespace TeaPie.Tests.Scripts;

public class PreProcessScriptStepShould
{
    [Fact]
    public async Task CallPreProcessMethodOnPreProcessorDuringExecution()
    {
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptWithSyntaxErrorPath);
        var accessor = new ScriptExecutionContextAccessor() { Context = context };
        context.RawContent = await File.ReadAllTextAsync(context.Script.File.Path);

        var processor = Substitute.For<IScriptPreProcessor>();

        var rootPath = ScriptIndex.RootSubFolderFullPath;
        var tempPath = Path.GetTempPath().NormalizePath();

        var pipeline = new ApplicationPipeline();
        var appContext = new ApplicationContextBuilder()
            .WithPath(rootPath)
            .WithTempFolderPath(tempPath)
            .Build();

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
