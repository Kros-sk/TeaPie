using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Http;

namespace TeaPie.Tests.Http;

public class ReadRequestFileStepShould
{
    [Fact]
    public async Task ThrowProperExceptionWhenRequestContextHasInvalidPath()
    {
        var context = RequestHelper.PrepareContext($"{Guid.NewGuid()}{Constants.RequestFileExtension}", false);

        var appContext = new ApplicationContext(
            RequestsIndex.RootFolderFullPath,
            Substitute.For<ILogger>(),
            Substitute.For<IServiceProvider>());

        var accessor = new RequestExecutionContextAccessor() { RequestExecutionContext = context };
        var step = new ReadRequestFileStep(accessor);

        await step.Invoking(async step => await step.Execute(appContext)).Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task AssignRawContentOfRequestFileCorrectly()
    {
        var context = RequestHelper.PrepareContext(RequestsIndex.RequestWithCommentsBodyAndHeadersPath, false);

        var appContext = new ApplicationContext(
            RequestsIndex.RootFolderFullPath,
            Substitute.For<ILogger>(),
            Substitute.For<IServiceProvider>());

        var accessor = new RequestExecutionContextAccessor() { RequestExecutionContext = context };
        var step = new ReadRequestFileStep(accessor);

        await step.Execute(appContext);

        var expectedContent = await File.ReadAllTextAsync(RequestsIndex.RequestWithCommentsBodyAndHeadersPath);

        context.RawContent.Should().Be(expectedContent);
    }
}
