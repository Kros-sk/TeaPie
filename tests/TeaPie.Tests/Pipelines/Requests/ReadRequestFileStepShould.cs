using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Extensions;
using TeaPie.Pipelines.Application;
using TeaPie.Pipelines.Requests;
using TeaPie.StructureExploration.IO;
using TeaPie.Tests.Requests;
using File = TeaPie.StructureExploration.IO.File;

namespace TeaPie.Tests.Pipelines.Requests;

public class ReadRequestFileStepShould
{
    [Fact]
    public async Task RawContentOfRequestFileShouldBeAssignedCorrectly()
    {
        var context = PrepareContext(RequestsIndex.RequestWithCommentsBodyAndHeadersPath);

        var appContext = new ApplicationContext(RequestsIndex.RootFolderFullPath, Substitute.For<ILogger>(), Substitute.For<IServiceProvider>());
        var accessor = new RequestExecutionContextAccessor() { RequestExecutionContext = context };
        var step = new ReadRequestFileStep(accessor);

        await step.Execute(appContext);

        var expectedContent = await System.IO.File.ReadAllTextAsync(RequestsIndex.RequestWithCommentsBodyAndHeadersPath);

        context.RawContent.Should().Be(expectedContent);
    }

    private static RequestExecutionContext PrepareContext(string path)
    {
        var folder = new Folder(RequestsIndex.RootFolderFullPath, RequestsIndex.RootFolderRelativePath, RequestsIndex.RootFolderName, null);
        var file = new File(
            path,
            RequestsIndex.RootFolderFullPath.TrimRootPath(Environment.CurrentDirectory),
            Path.GetFileName(path),
            folder);

        return new RequestExecutionContext(file);
    }
}
