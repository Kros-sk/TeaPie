using FluentAssertions;
using TeaPie.Http;
using TeaPie.TestCases;

namespace TeaPie.Tests.Http;

public class ReadHttpFileStepShould
{
    [Fact]
    public async Task ThrowProperExceptionWhenRequestContextHasInvalidPath()
    {
        var context = RequestHelper.PrepareTestCaseContext($"{Guid.NewGuid()}{Constants.RequestFileExtension}", false);

        var appContext = new ApplicationContextBuilder()
            .WithPath(RequestsIndex.RootFolderFullPath)
            .Build();

        var accessor = new TestCaseExecutionContextAccessor() { TestCaseExecutionContext = context };
        var step = new ReadHttpFileStep(accessor);

        await step.Invoking(async step => await step.Execute(appContext)).Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task AssignRawContentOfRequestFileCorrectly()
    {
        var context = RequestHelper.PrepareTestCaseContext(RequestsIndex.RequestWithFullStructure, false);

        var appContext = new ApplicationContextBuilder()
            .WithPath(RequestsIndex.RootFolderFullPath)
            .Build();

        var accessor = new TestCaseExecutionContextAccessor() { TestCaseExecutionContext = context };
        var step = new ReadHttpFileStep(accessor);

        await step.Execute(appContext);

        var expectedContent = await File.ReadAllTextAsync(RequestsIndex.RequestWithFullStructure);

        context.RequestsFileContent.Should().Be(expectedContent);
    }
}
