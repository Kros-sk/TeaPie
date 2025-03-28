using NSubstitute;
using TeaPie.Scripts;
using TeaPie.StructureExploration;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.Tests.Scripts;

public class PreProcessScriptStepShould
{
    [Fact]
    public async Task CallPreProcessMethodOnPreProcessorDuringExecution()
    {
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptWithSyntaxErrorPath);
        var accessor = new ScriptExecutionContextAccessor() { Context = context };
        context.RawContent = await System.IO.File.ReadAllTextAsync(context.Script.File.Path);

        var processor = Substitute.For<IScriptPreProcessor>();

        var rootPath = ScriptIndex.RootSubFolderFullPath;
        var tempPath = Path.GetTempPath().NormalizePath();

        var pipeline = new ApplicationPipeline();
        var appContext = new ApplicationContextBuilder()
            .WithPath(rootPath)
            .WithTempFolderPath(tempPath)
            .Build();

        var step = new PreProcessScriptStep(pipeline, accessor, processor, Substitute.For<IExternalFileRegistry>());
        await step.Execute(appContext);

        await processor.Received(1).ProcessScript(context, Arg.Any<List<ScriptReference>>());
    }
}
