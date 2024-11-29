using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Scripts;

namespace TeaPie.Tests.Scripts;

public class ReadScriptFileStepShould
{
    [Fact]
    public async Task ScriptContextWithInvalidPathShouldThrowProperException()
    {
        var context = ScriptHelper.GetScriptExecutionContext($"{Guid.NewGuid()}{Constants.ScriptFileExtension}");

        var appContext = new ApplicationContext(
            ScriptIndex.RootSubFolderFullPath,
            Substitute.For<ILogger>(),
            Substitute.For<IServiceProvider>());

        var accessor = new ScriptExecutionContextAccessor() { ScriptExecutionContext = context };
        var step = new ReadScriptFileStep(accessor);

        await step.Invoking(async step => await step.Execute(appContext)).Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task RawContentOfScriptFileShouldBeAssignedCorrectly()
    {
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptWithMultipleLoadAndNuGetDirectivesPath);

        var appContext = new ApplicationContext(
            ScriptIndex.RootSubFolderFullPath,
            Substitute.For<ILogger>(),
            Substitute.For<IServiceProvider>());

        var accessor = new ScriptExecutionContextAccessor() { ScriptExecutionContext = context };
        var step = new ReadScriptFileStep(accessor);

        await step.Execute(appContext);

        var expectedContent = await File.ReadAllTextAsync(ScriptIndex.ScriptWithMultipleLoadAndNuGetDirectivesPath);

        context.RawContent.Should().Be(expectedContent);
    }
}
