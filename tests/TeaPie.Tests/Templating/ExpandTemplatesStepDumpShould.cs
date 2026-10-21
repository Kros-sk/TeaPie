using FluentAssertions;
using NSubstitute;
using Spectre.Console;
using Spectre.Console.Testing;
using TeaPie.Templating;
using TeaPie.Tests.Http;
using TeaPie.TestCases;

namespace TeaPie.Tests.Templating;

[Collection(nameof(NonParallelCollection))]
public class ExpandTemplatesStepDumpShould : IDisposable
{
    private const string TemplatedContent = "{% for x in Xs %}BODY{% endfor %}";
    private const string ExpandedContent = "### Expanded\nGET https://example.test/expanded\n";

    private readonly string _dumpDirectory =
        Path.Combine(Path.GetTempPath(), "teapie-dump-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task WriteTheExpandedContentToTheDumpDirectoryKeepingTheRelativePath()
    {
        var (step, testCaseContext) = CreateStep(TemplatedContent);
        var appContext = CreateApplicationContext(_dumpDirectory);

        await step.Execute(appContext);

        var dumpFilePath = Path.Combine(_dumpDirectory, testCaseContext.TestCase.RequestsFile.RelativePath);
        File.ReadAllText(dumpFilePath).Should().Be(ExpandedContent);
    }

    [Fact]
    public async Task WriteTheExpandedContentToTheConsoleWhenNoDumpDirectoryIsGiven()
    {
        var (step, testCaseContext) = CreateStep(TemplatedContent);
        var appContext = CreateApplicationContext(string.Empty);
        var testConsole = new TestConsole();
        var originalConsole = AnsiConsole.Console;
        AnsiConsole.Console = testConsole;

        try
        {
            await step.Execute(appContext);
        }
        finally
        {
            AnsiConsole.Console = originalConsole;
        }

        testConsole.Output.Should().Contain(Path.GetFileName(testCaseContext.TestCase.RequestsFile.RelativePath))
            .And.Contain("GET https://example.test/expanded");
    }

    [Fact]
    public async Task NotDumpAFileWithoutTemplates()
    {
        var (step, _) = CreateStep("GET https://example.test/plain");
        var appContext = CreateApplicationContext(_dumpDirectory);

        await step.Execute(appContext);

        Directory.Exists(_dumpDirectory).Should().BeFalse();
    }

    [Fact]
    public async Task NotDumpAnythingWhenTheDumpIsNotRequested()
    {
        var (step, _) = CreateStep(TemplatedContent);
        var appContext = new ApplicationContextBuilder().WithPath(RequestsIndex.RootFolderFullPath).Build();

        await step.Execute(appContext);

        Directory.Exists(_dumpDirectory).Should().BeFalse();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RefuseToWriteADumpInsideTheTestedCollection(bool dumpIntoCollectionParent)
    {
        var (step, testCaseContext) = CreateStep(TemplatedContent);
        var relativePath = testCaseContext.TestCase.RequestsFile.RelativePath;
        var collectionPath = dumpIntoCollectionParent
            ? Path.Combine(_dumpDirectory, relativePath.Split('/', '\\')[0])
            : _dumpDirectory;
        Directory.CreateDirectory(collectionPath);
        var appContext = new ApplicationContextBuilder()
            .WithPath(collectionPath)
            .WithExpandedContentDump(_dumpDirectory)
            .Build();

        var act = () => step.Execute(appContext);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage($"*{collectionPath}*outside*");
        File.Exists(Path.Combine(_dumpDirectory, relativePath)).Should().BeFalse();
    }

    public void Dispose()
    {
        if (Directory.Exists(_dumpDirectory))
        {
            Directory.Delete(_dumpDirectory, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    private static (ExpandTemplatesStep Step, TestCaseExecutionContext Context) CreateStep(string content)
    {
        var context = RequestHelper.PrepareTestCaseContext(RequestsIndex.RequestWithFullStructure, false);
        context.RequestsFileContent = content;

        var expander = Substitute.For<ITemplateExpander>();
        expander.Expand(content, context.TestCase.RequestsFile.RelativePath)
            .Returns(content.Contains("{%") ? ExpandedContent : content);

        var accessor = new TestCaseExecutionContextAccessor { Context = context };
        return (new ExpandTemplatesStep(accessor, expander), context);
    }

    private static ApplicationContext CreateApplicationContext(string dumpPath)
        => new ApplicationContextBuilder()
            .WithPath(RequestsIndex.RootFolderFullPath)
            .WithExpandedContentDump(dumpPath)
            .Build();
}
