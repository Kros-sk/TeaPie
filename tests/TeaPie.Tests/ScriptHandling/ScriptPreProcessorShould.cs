using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TeaPie.Exceptions;
using TeaPie.Parsing;
using TeaPie.ScriptHandling;

namespace TeaPie.Tests.ScriptHandling;

public sealed class ScriptPreProcessorShould
{
    private const string RootFolderName = "Demo";
    private const string RootSubFolder = "Scripts";
    private static readonly string _rootFolderPath = Path.Combine(Environment.CurrentDirectory, RootFolderName);

    private static readonly string _rootSubFolderPath =
        Path.Combine(Environment.CurrentDirectory, RootFolderName, RootSubFolder);

    private readonly string _emptyScriptPath =
        Path.Combine(_rootSubFolderPath, $"emptyScript{Constants.ScriptFileExtension}");

    private readonly string _plainScriptPath =
        Path.Combine(_rootSubFolderPath, $"plainScript{Constants.ScriptFileExtension}");

    private readonly string _scriptWithNonExistingScriptLoadDirectivePath =
        Path.Combine(_rootSubFolderPath, $"scriptWithNonExistingScriptLoadDirective{Constants.ScriptFileExtension}");

    private readonly string _scriptWithOneLoadDirectivePath =
        Path.Combine(_rootSubFolderPath, $"scriptWithOneLoadDirective{Constants.ScriptFileExtension}");

    private readonly string _scriptWithMultipleLoadDirectives =
        Path.Combine(_rootSubFolderPath, $"scriptWithMultipleLoadDirectives{Constants.ScriptFileExtension}");

    private readonly string _scriptWithInvalidNugetDirectivePath =
        Path.Combine(_rootSubFolderPath, $"scriptWithInvalidNugetDirective{Constants.ScriptFileExtension}");

    private readonly string _scriptWithOneNugetDirectivePath =
        Path.Combine(_rootSubFolderPath, $"scriptWithOneNugetDirective{Constants.ScriptFileExtension}");

    private readonly string _scriptWithMultipleNugetDirectivesPath =
        Path.Combine(_rootSubFolderPath, $"scriptWithMultipleNugetDirectives{Constants.ScriptFileExtension}");

    private readonly string _scriptWithMultipleLoadAndNugetDirectivesPath =
        Path.Combine(_rootSubFolderPath, $"scriptWithMultipleLoadAndNugetDirectives{Constants.ScriptFileExtension}");

    private readonly string _tempFolderPath = Path.Combine(Path.GetTempPath(), Constants.ApplicationName);

    [Fact]
    public async Task EmptyScriptFileShouldNotCauseProblem()
    {
        var processor = CreateScriptPreProcessor();
        List<string> referencedScripts = [];
        var processedContent = await PreProcessScript(processor, _emptyScriptPath, referencedScripts);

        processedContent.Should().BeEquivalentTo(string.Empty);
    }

    [Fact]
    public async Task ScriptWithoutAnyDirectivesShouldRemainTheSame()
    {
        var processor = CreateScriptPreProcessor();
        List<string> referencedScripts = [];
        var content = await File.ReadAllTextAsync(_plainScriptPath);

        var processedContent = await PreProcessScript(processor, _plainScriptPath, referencedScripts);

        processedContent.Should().BeEquivalentTo(content);
    }

    [Fact]
    public async Task ScriptWithNonExistingScriptReferenceShouldThrowException()
    {
        var processor = CreateScriptPreProcessor();
        List<string> referencedScripts = [];

        await processor.Invoking(async processor => await processor.ProcessScript(
            _scriptWithNonExistingScriptLoadDirectivePath,
            await File.ReadAllTextAsync(_scriptWithNonExistingScriptLoadDirectivePath),
            _rootFolderPath,
            _tempFolderPath,
            referencedScripts))
            .Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task ScriptWithOneLoadDirectiveShouldBeResolvedCorrectly()
    {
        var processor = CreateScriptPreProcessor();

        List<string> referencedScripts = [];

        var content = await File.ReadAllLinesAsync(_scriptWithOneLoadDirectivePath);

        var processedContent = await PreProcessScript(processor, _scriptWithOneLoadDirectivePath, referencedScripts);

        var contentWithoutDirective = string.Join(Environment.NewLine, content[1..]);

        referencedScripts.Should().HaveCount(1);

        var expectedDirective = GetExpectedDirectives("init")[0];

        processedContent.Should().Contain(expectedDirective + Environment.NewLine + contentWithoutDirective);
    }

    [Fact]
    public async Task ScriptWithMultipleLoadDirectivesShouldBeResolvedCorrectly()
    {
        var processor = CreateScriptPreProcessor();
        const int numberOfDirectives = 3;
        var scriptRelativePathsWithoutFileExtensions = new string[]
        {
            "init",
            $"Nested{Path.DirectorySeparatorChar}first",
            $"Nested{Path.DirectorySeparatorChar}second"
        };

        List<string> referencedScripts = [];
        var processedContent = await PreProcessScript(processor, _scriptWithMultipleLoadDirectives, referencedScripts);

        var expectedDirectives =
            string.Join(Environment.NewLine, GetExpectedDirectives(scriptRelativePathsWithoutFileExtensions));

        referencedScripts.Should().HaveCount(numberOfDirectives);

        foreach (var path in scriptRelativePathsWithoutFileExtensions)
        {
            referencedScripts.Should().Contain(Path.Combine(_rootSubFolderPath, path + Constants.ScriptFileExtension));
        }

        processedContent.Should().Contain(expectedDirectives);
    }

    [Fact]
    public async Task ScriptWithInvalidNugetDirectiveShouldThrowException()
    {
        var nugetHandler = new NugetPackageHandler(NullLogger.Instance);
        var processor = CreateScriptPreProcessor(nugetHandler);

        List<string> referencedScripts = [];

        await processor.Invoking(async processor => await processor.ProcessScript(
            _scriptWithInvalidNugetDirectivePath,
            await File.ReadAllTextAsync(_scriptWithInvalidNugetDirectivePath),
            _rootFolderPath,
            _tempFolderPath,
            referencedScripts))
            .Should().ThrowAsync<NugetPackageNotFoundException>();
    }

    [Fact]
    public async Task ScriptWithOneNugetDirectiveShouldBeHandledProperly()
    {
        var nugetHandler = Substitute.For<INugetPackageHandler>();
        var processor = CreateScriptPreProcessor(nugetHandler);
        List<string> referencedScripts = [];

        var processedContent = await PreProcessScript(processor, _scriptWithOneNugetDirectivePath, referencedScripts);

        await nugetHandler.Received(1).HandleNugetPackages(Arg.Any<List<NugetPackageDescription>>());
        processedContent.Should().NotContain(ParsingConstants.NugetDirective);
    }

    [Fact]
    public async Task ScriptWithMultipleNugetDirectivesShouldBeHandledProperly()
    {
        var nugetHandler = Substitute.For<INugetPackageHandler>();
        var processor = CreateScriptPreProcessor(nugetHandler);
        List<string> referencedScripts = [];

        var processedContent = await PreProcessScript(processor, _scriptWithMultipleNugetDirectivesPath, referencedScripts);

        await nugetHandler.Received(1).HandleNugetPackages(Arg.Any<List<NugetPackageDescription>>());
        processedContent.Should().NotContain(ParsingConstants.NugetDirective);
    }

    [Fact]
    public async Task ScriptWithMultipleLoadAndNugetDirectivesShouldBeHandledProperly()
    {
        var nugetHandler = Substitute.For<INugetPackageHandler>();
        var processor = CreateScriptPreProcessor(nugetHandler);
        var scriptRelativePathsWithoutFileExtensions = new string[]
        {
            "init",
            $"Nested{Path.DirectorySeparatorChar}first",
            $"Nested{Path.DirectorySeparatorChar}second"
        };
        const int numberOfLoadDirectives = 3;
        List<string> referencedScripts = [];

        var processedContent =
            await PreProcessScript(processor, _scriptWithMultipleLoadAndNugetDirectivesPath, referencedScripts);

        var expectedLoadDirectives =
            string.Join(Environment.NewLine, GetExpectedDirectives(scriptRelativePathsWithoutFileExtensions));

        referencedScripts.Should().HaveCount(numberOfLoadDirectives);
        processedContent.Should().Contain(expectedLoadDirectives);

        referencedScripts.Should().HaveCount(numberOfLoadDirectives);

        foreach (var path in scriptRelativePathsWithoutFileExtensions)
        {
            referencedScripts.Should().Contain(Path.Combine(_rootSubFolderPath, path + Constants.ScriptFileExtension));
        }

        await nugetHandler.Received(1).HandleNugetPackages(Arg.Any<List<NugetPackageDescription>>());
        processedContent.Should().NotContain(ParsingConstants.NugetDirective);
    }

    private async Task<string> PreProcessScript(ScriptPreProcessor processor, string scriptPath, List<string> referencedScripts)
        => await processor.ProcessScript(
            scriptPath,
            await File.ReadAllTextAsync(scriptPath),
            _rootFolderPath,
            _tempFolderPath,
            referencedScripts);

    private List<string> GetExpectedDirectives(params string[] names)
    {
        List<string> list = [];
        var tmpBasePath = Path.Combine(_tempFolderPath, RootFolderName, RootSubFolder);

        for (var i = 0; i < names.Length; i++)
        {
            list.Add($"{ParsingConstants.LoadScriptDirective} " +
                $"\"{tmpBasePath}{Path.DirectorySeparatorChar}{names[i]}{Constants.ScriptFileExtension}\"");
        }

        return list;
    }

    private static ScriptPreProcessor CreateScriptPreProcessor(INugetPackageHandler? nugetPackageHandler = null)
    {
        var logger = NullLogger.Instance;

        if (nugetPackageHandler is null)
        {
            return new(new NugetPackageHandler(logger), logger);
        }
        else
        {
            return new(nugetPackageHandler, logger);
        }
    }
}
