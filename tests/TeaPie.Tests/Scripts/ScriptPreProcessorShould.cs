using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Scripts;
using TeaPie.StructureExploration;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.Tests.Scripts;

public sealed class ScriptPreProcessorShould
{
    private readonly PathProvider _pathProvider;

    public ScriptPreProcessorShould()
    {
        _pathProvider = new PathProvider();
        _pathProvider.UpdatePaths(ScriptIndex.RootSubFolderFullPath, Constants.SystemTemporaryFolderPath);
    }

    [Fact]
    public async Task PreProcessEmptyScriptWithoutAnyProblem()
    {
        var processor = CreateScriptPreProcessor();
        List<ScriptReference> referencedScripts = [];
        var processedContent = await PreProcessScript(processor, ScriptIndex.EmptyScriptPath, referencedScripts);

        processedContent.Should().BeEquivalentTo(string.Empty);
    }

    [Fact]
    public async Task ReturnSameScriptWhenPreProcessingScriptWithoutAnyDirectives()
    {
        var processor = CreateScriptPreProcessor();
        List<ScriptReference> referencedScripts = [];
        var content = await System.IO.File.ReadAllLinesAsync(ScriptIndex.PlainScriptPath);
        var trimmedContent = content.Where(l => !string.IsNullOrEmpty(l));

        var processedContent = await PreProcessScript(processor, ScriptIndex.PlainScriptPath, referencedScripts);

        processedContent.Should().BeEquivalentTo(string.Join(Environment.NewLine, trimmedContent));
    }

    [Fact]
    public async Task ThrowProperExceptionWhenPreProcessingScriptWithReferenceToNonExistingScript()
    {
        var processor = CreateScriptPreProcessor();
        List<ScriptReference> referencedScripts = [];

        var scriptContext = new ScriptExecutionContext(
            new Script(
                new InternalFile(
                    ScriptIndex.ScriptWithNonExistingScriptLoadDirectivePath,
                    string.Empty,
                    new Folder(string.Empty, string.Empty, string.Empty, null))))
        {
            RawContent =
                await System.IO.File.ReadAllTextAsync(ScriptIndex.ScriptWithNonExistingScriptLoadDirectivePath)
        };

        await processor.Invoking(async processor => await processor.ProcessScript(scriptContext, referencedScripts))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task PreProcessScriptWithOneLoadDirectiveCorrectly()
    {
        var processor = CreateScriptPreProcessor();

        List<ScriptReference> referencedScripts = [];

        var content = await System.IO.File.ReadAllLinesAsync(ScriptIndex.ScriptWithOneLoadDirectivePath);

        var processedContent = await PreProcessScript(processor, ScriptIndex.ScriptWithOneLoadDirectivePath, referencedScripts);

        var contentWithoutDirective = string.Join(Environment.NewLine, content[1..].Where(l => !string.IsNullOrEmpty(l)));

        referencedScripts.Should().HaveCount(1);

        var expectedDirective = GetExpectedDirectives("init")[0];

        processedContent.Should().Contain(expectedDirective + Environment.NewLine + contentWithoutDirective);
    }

    [Fact]
    public async Task PreProcessScriptWithMultipleLoadDirectivesCorrectly()
    {
        var processor = CreateScriptPreProcessor();
        const int numberOfDirectives = 3;
        var scriptRelativePathsWithoutFileExtensions = new string[]
        {
            "init",
            Path.Combine("Nested", "first"),
            Path.Combine("Nested", "second")
        };

        List<ScriptReference> referencedScripts = [];
        var processedContent =
            await PreProcessScript(processor, ScriptIndex.ScriptWithMultipleLoadDirectives, referencedScripts);

        var expectedDirectives =
            string.Join(Environment.NewLine, GetExpectedDirectives(scriptRelativePathsWithoutFileExtensions));

        referencedScripts.Should().HaveCount(numberOfDirectives);

        foreach (var path in scriptRelativePathsWithoutFileExtensions)
        {
            referencedScripts.Select(sr => sr.RealPath)
                .Should()
                .Contain(Path.Combine(_pathProvider.RootPath, path + Constants.ScriptFileExtension));
        }

        processedContent.Should().Contain(expectedDirectives);
    }

    [Fact]
    public async Task ThrowProperExceptionWhenCompilingScriptWithInvalidNuGetDirective()
    {
        var nugetHandler = GetNuGetHandler();

        var processor = CreateScriptPreProcessor(nugetHandler);

        List<ScriptReference> referencedScripts = [];

        var scriptContext = new ScriptExecutionContext(
            new Script(
                new InternalFile(
                    ScriptIndex.ScriptWithInvalidNuGetDirectivePath,
                    string.Empty,
                    new Folder(string.Empty, string.Empty, string.Empty, null))))
        {
            RawContent = await System.IO.File.ReadAllTextAsync(ScriptIndex.ScriptWithInvalidNuGetDirectivePath)
        };

        await processor.Invoking(async processor => await processor.ProcessScript(scriptContext, referencedScripts))
            .Should().ThrowAsync<NuGetPackageNotFoundException>();
    }

    [Fact]
    public async Task PreProcessScriptWithOneNuGetDirectiveCorrectly()
    {
        var nugetHandler = Substitute.For<INuGetPackageHandler>();
        var processor = CreateScriptPreProcessor(nugetHandler);
        List<ScriptReference> referencedScripts = [];

        var processedContent = await PreProcessScript(processor, ScriptIndex.ScriptWithOneNuGetDirectivePath, referencedScripts);

        await nugetHandler.Received(1).HandleNuGetPackage(new NuGetPackageDescription("Newtonsoft.Json", "13.0.3"));
        processedContent.Should().NotContain(ScriptPreProcessorConstants.NuGetDirective);
    }

    [Fact]
    public async Task PreProcessScriptWithMultipleNuGetDirectivesCorrectly()
    {
        var nugetHandler = Substitute.For<INuGetPackageHandler>();
        var processor = CreateScriptPreProcessor(nugetHandler);
        List<ScriptReference> referencedScripts = [];

        var processedContent =
            await PreProcessScript(processor, ScriptIndex.ScriptWithMultipleNuGetDirectivesPath, referencedScripts);

        await nugetHandler.Received(4).HandleNuGetPackage(Arg.Any<NuGetPackageDescription>());
        processedContent.Should().NotContain(ScriptPreProcessorConstants.NuGetDirective);
    }

    [Fact]
    public async Task PreProcessScriptWithDuplicatedNuGetDirectivesCorrectly()
    {
        var nugetHandler = Substitute.For<INuGetPackageHandler>();
        var processor = CreateScriptPreProcessor(nugetHandler);
        List<ScriptReference> referencedScripts = [];

        var processedContent =
            await PreProcessScript(processor, ScriptIndex.ScriptWithDuplicatedNuGetDirectivePath, referencedScripts);

        await nugetHandler.Received(2).HandleNuGetPackage(new NuGetPackageDescription("Newtonsoft.Json", "13.0.3"));
        processedContent.Should().NotContain(ScriptPreProcessorConstants.NuGetDirective);
    }

    [Fact]
    public async Task PreProcessScriptWithMultipleLoadAndNuGetDirectivesCorrectly()
    {
        var nugetHandler = Substitute.For<INuGetPackageHandler>();
        var processor = CreateScriptPreProcessor(nugetHandler);
        var scriptRelativePathsWithoutFileExtensions = new string[]
        {
            "init",
            Path.Combine("Nested", "first"),
            Path.Combine("Nested", "second")
        };

        const int numberOfLoadDirectives = 3;
        List<ScriptReference> referencedScripts = [];

        var processedContent =
            await PreProcessScript(processor, ScriptIndex.ScriptWithMultipleLoadAndNuGetDirectivesPath, referencedScripts);

        var expectedLoadDirectives =
            string.Join(Environment.NewLine, GetExpectedDirectives(scriptRelativePathsWithoutFileExtensions));

        referencedScripts.Should().HaveCount(numberOfLoadDirectives);
        processedContent.Should().Contain(expectedLoadDirectives);

        referencedScripts.Should().HaveCount(numberOfLoadDirectives);

        foreach (var path in scriptRelativePathsWithoutFileExtensions)
        {
            referencedScripts.Select(sr => sr.RealPath)
                .Should()
                .Contain(Path.Combine(_pathProvider.RootPath, path + Constants.ScriptFileExtension));
        }

        await nugetHandler.Received(3).HandleNuGetPackage(Arg.Any<NuGetPackageDescription>());
        processedContent.Should().NotContain(ScriptPreProcessorConstants.NuGetDirective);
    }

    private static async Task<string> PreProcessScript(
        ScriptPreProcessor processor, string scriptPath, List<ScriptReference> referencedScripts)
    {
        var scriptContext = new ScriptExecutionContext(
            new Script(
                new InternalFile(
                    scriptPath,
                    string.Empty,
                    new Folder(string.Empty, string.Empty, string.Empty, null))))
        {
            RawContent = await System.IO.File.ReadAllTextAsync(scriptPath)
        };

        await processor.ProcessScript(
            scriptContext,
            referencedScripts);

        return scriptContext.ProcessedContent!;
    }

    private List<string> GetExpectedDirectives(params string[] names)
    {
        List<string> list = [];

        for (var i = 0; i < names.Length; i++)
        {
            list.Add($"{ScriptPreProcessorConstants.LoadScriptDirective} " +
                $"\"{_pathProvider.TempFolderPath}{Path.DirectorySeparatorChar}{names[i]}{Constants.ScriptFileExtension}\"");
        }

        return list;
    }

    private static NuGetPackageHandler GetNuGetHandler()
        => new(Substitute.For<IPathProvider>(), Substitute.For<ILogger<NuGetPackageHandler>>(), NuGet.Common.NullLogger.Instance);

    private ScriptPreProcessor CreateScriptPreProcessor(INuGetPackageHandler? nugetPackageHandler = null)
    {
        var resolversProvider = new ScriptLineResolversProvider(
            nugetPackageHandler ?? Substitute.For<INuGetPackageHandler>(),
            new PathResolver(_pathProvider),
            new TemporaryPathResolver(_pathProvider, new RelativePathResolver()),
            Substitute.For<IExternalFileRegistry>(),
            _pathProvider);

        return new(resolversProvider);
    }
}
