using FluentAssertions;
using Spectre.Console.Testing;
using TeaPie.StructureExploration;
using TeaPie.Templating;
using TeaPie.Tests.Http;

namespace TeaPie.Tests.Templating;

public class ExpandedContentDumperShould : IDisposable
{
    private const string ExpandedContent = "### Expanded\nGET https://example.test/expanded\n";

    private readonly string _dumpDirectory =
        Path.Combine(Path.GetTempPath(), "teapie-dump-tests", Guid.NewGuid().ToString("N"));

    private readonly TestConsole _console = new();

    [Fact]
    public void WriteTheExpandedContentToTheDumpDirectoryKeepingTheRelativePath()
    {
        var testCase = CreateTestCase();

        CreateDumper().Dump(CreateApplicationContext(_dumpDirectory), testCase, ExpandedContent);

        var dumpFilePath = Path.Combine(_dumpDirectory, testCase.RequestsFile.RelativePath);
        System.IO.File.ReadAllText(dumpFilePath).Should().Be(ExpandedContent);
        _console.Output.Should().BeEmpty();
    }

    [Theory]
    [InlineData("GET.http", null, "GET.http")]
    [InlineData("Seed.tp", "Seed", "Seed.tp.http")]
    [InlineData("Seed.tp", "seed", "Seed.tp.http")]
    [InlineData("Seed.tp", "First", "Seed.tp.First.http")]
    [InlineData("Seed.tp", "Create user/admin", "Seed.tp.Create user_admin.http")]
    public void NameTheDumpFileAfterTheRequestsFileAndTheTestCaseOfATpFile(
        string fileName, string? tpTestCaseName, string expectedDumpFileName)
    {
        var testCase = CreateTestCase(fileName, tpTestCaseName);

        CreateDumper().Dump(CreateApplicationContext(_dumpDirectory), testCase, ExpandedContent);

        var expectedDumpFilePath = Path.Combine(
            _dumpDirectory, Path.GetDirectoryName(testCase.RequestsFile.RelativePath)!, expectedDumpFileName);
        Directory.GetFiles(_dumpDirectory, "*", SearchOption.AllDirectories)
            .Should().Equal(expectedDumpFilePath);
    }

    [Theory]
    [InlineData("GET.http", null, "GET.http ")]
    [InlineData("Seed.tp", "Seed", "Seed.tp ")]
    [InlineData("Seed.tp", "First", "Seed.tp (First) ")]
    public void WriteTheExpandedContentToTheConsoleWhenNoDumpDirectoryIsGiven(
        string fileName, string? tpTestCaseName, string expectedHeaderEnd)
    {
        var testCase = CreateTestCase(fileName, tpTestCaseName);

        CreateDumper().Dump(CreateApplicationContext(string.Empty), testCase, ExpandedContent);

        _console.Output.Should().Contain(expectedHeaderEnd)
            .And.Contain("GET https://example.test/expanded");
        Directory.Exists(_dumpDirectory).Should().BeFalse();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RefuseToWriteADumpInsideTheTestedCollection(bool dumpIntoCollectionParent)
    {
        var testCase = CreateTestCase();
        var relativePath = testCase.RequestsFile.RelativePath;
        var collectionPath = dumpIntoCollectionParent
            ? Path.Combine(_dumpDirectory, relativePath.Split('/', '\\')[0])
            : _dumpDirectory;
        Directory.CreateDirectory(collectionPath);
        var appContext = new ApplicationContextBuilder()
            .WithPath(collectionPath)
            .WithExpandedContentDump(_dumpDirectory)
            .Build();

        var act = () => CreateDumper().Dump(appContext, testCase, ExpandedContent);

        act.Should().Throw<InvalidOperationException>().WithMessage($"*{collectionPath}*outside*");
        System.IO.File.Exists(Path.Combine(_dumpDirectory, relativePath)).Should().BeFalse();
    }

    [Theory]
    [InlineData("", "", true)]
    [InlineData("Demo", "", true)]
    [InlineData("Demo", "Demo/out", true)]
    [InlineData("Demo", "out", false)]
    [InlineData("Demo", null, false)]
    public void CheckBeforeTheRunThatNoDumpWouldBeWrittenInsideTheTestedCollection(
        string collectionSubPath, string? dumpSubPath, bool expectedRefusal)
    {
        var testCase = CreateTestCase();
        var collectionPath = Path.Combine(_dumpDirectory, collectionSubPath);
        Directory.CreateDirectory(collectionPath);
        var structure = new CollectionStructure();
        structure.TryAddTestCase(testCase);
        var appContext = new ApplicationContextBuilder()
            .WithPath(collectionPath)
            .WithExpandedContentDump(dumpSubPath is null ? string.Empty : Path.Combine(_dumpDirectory, dumpSubPath))
            .Build();
        appContext.CollectionStructure = structure;

        var act = () => CreateDumper().EnsureDumpsAreOutsideOfTestedCollection(appContext);

        if (expectedRefusal)
        {
            act.Should().Throw<InvalidOperationException>().WithMessage($"*{collectionPath}*outside*");
        }
        else
        {
            act.Should().NotThrow();
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_dumpDirectory))
        {
            Directory.Delete(_dumpDirectory, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    private ExpandedContentDumper CreateDumper() => new(_console);

    private static TestCase CreateTestCase()
        => RequestHelper.PrepareTestCaseContext(RequestsIndex.RequestWithFullStructure, false).TestCase;

    private static TestCase CreateTestCase(string fileName, string? tpTestCaseName)
    {
        var file = RequestHelper.GetFile(Path.Combine(RequestsIndex.RootFolderFullPath, fileName));

        return tpTestCaseName is null
            ? new TestCase(file)
            : new TestCase(file) { Name = tpTestCaseName, IsFromTpFile = true };
    }

    private static ApplicationContext CreateApplicationContext(string dumpPath)
        => new ApplicationContextBuilder()
            .WithPath(RequestsIndex.RootFolderFullPath)
            .WithExpandedContentDump(dumpPath)
            .Build();
}
