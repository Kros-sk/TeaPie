using FluentAssertions;
using TeaPie.Scripts;

namespace TeaPie.Tests.Scripts;

public class ReadScriptFileStepShould
{
    [Fact]
    public async Task ThrowProperExceptionWhenScriptContextHasInvalidPath()
    {
        var context = ScriptHelper.GetScriptExecutionContext($"{Guid.NewGuid()}{Constants.ScriptFileExtension}");

        var appContext = new ApplicationContextBuilder()
            .WithPath(ScriptIndex.RootSubFolderFullPath)
            .Build();

        var accessor = new ScriptExecutionContextAccessor() { Context = context };
        var step = new ReadScriptFileStep(accessor);

        await step.Invoking(async step => await step.Execute(appContext)).Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task AssignRawContentOfScriptFileCorrectly()
    {
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptWithMultipleLoadAndNuGetDirectivesPath);

        var appContext = new ApplicationContextBuilder()
            .WithPath(ScriptIndex.RootSubFolderFullPath)
            .Build();

        var accessor = new ScriptExecutionContextAccessor() { Context = context };
        var step = new ReadScriptFileStep(accessor);

        await step.Execute(appContext);

        var expectedContent = await File.ReadAllTextAsync(ScriptIndex.ScriptWithMultipleLoadAndNuGetDirectivesPath);

        context.RawContent.Should().Be(expectedContent);
    }
}
