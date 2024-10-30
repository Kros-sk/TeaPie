using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Exceptions;
using TeaPie.Parsing;
using TeaPie.ScriptHandling;

namespace TeaPie.Tests.ScriptHandling;

public sealed class ScriptPreProcessorShould
{
    private readonly string _tempFolderPath = Path.Combine(Path.GetTempPath(), Constants.ApplicationName);

    [Fact]
    public async Task EmptyScriptFileShouldNotCauseProblem()
    {
        var processor = CreateScriptPreProcessor();
        List<string> referencedScripts = [];
        var processedContent = await PreProcessScript(processor, ScriptIndex.EmptyScriptPath, referencedScripts);

        processedContent.Should().BeEquivalentTo(string.Empty);
    }

    [Fact]
    public async Task ScriptWithoutAnyDirectivesShouldRemainTheSame()
    {
        var processor = CreateScriptPreProcessor();
        List<string> referencedScripts = [];
        var content = await File.ReadAllTextAsync(ScriptIndex.PlainScriptPath);

        var processedContent = await PreProcessScript(processor, ScriptIndex.PlainScriptPath, referencedScripts);

        processedContent.Should().BeEquivalentTo(content);
    }

    [Fact]
    public async Task ScriptWithNonExistingScriptReferenceShouldThrowException()
    {
        var processor = CreateScriptPreProcessor();
        List<string> referencedScripts = [];

        await processor.Invoking(async processor => await processor.ProcessScript(
            ScriptIndex.ScriptWithNonExistingScriptLoadDirectivePath,
            await File.ReadAllTextAsync(ScriptIndex.ScriptWithNonExistingScriptLoadDirectivePath),
            ScriptIndex.RootFolderPath,
            _tempFolderPath,
            referencedScripts))
            .Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task ScriptWithOneLoadDirectiveShouldBeResolvedCorrectly()
    {
        var processor = CreateScriptPreProcessor();

        List<string> referencedScripts = [];

        var content = await File.ReadAllLinesAsync(ScriptIndex.ScriptWithOneLoadDirectivePath);

        var processedContent = await PreProcessScript(processor, ScriptIndex.ScriptWithOneLoadDirectivePath, referencedScripts);

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
        var processedContent =
            await PreProcessScript(processor, ScriptIndex.ScriptWithMultipleLoadDirectives, referencedScripts);

        var expectedDirectives =
            string.Join(Environment.NewLine, GetExpectedDirectives(scriptRelativePathsWithoutFileExtensions));

        referencedScripts.Should().HaveCount(numberOfDirectives);

        foreach (var path in scriptRelativePathsWithoutFileExtensions)
        {
            referencedScripts.Should()
                .Contain(Path.Combine(ScriptIndex.RootSubFolderPath, path + Constants.ScriptFileExtension));
        }

        processedContent.Should().Contain(expectedDirectives);
    }

    [Fact]
    public async Task ScriptWithInvalidNugetDirectiveShouldThrowException()
    {
        var nugetHandler = GetNugetHandler();

        var processor = CreateScriptPreProcessor(nugetHandler);

        List<string> referencedScripts = [];

        await processor.Invoking(async processor => await processor.ProcessScript(
            ScriptIndex.ScriptWithInvalidNugetDirectivePath,
            await File.ReadAllTextAsync(ScriptIndex.ScriptWithInvalidNugetDirectivePath),
            ScriptIndex.RootFolderPath,
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

        var processedContent = await PreProcessScript(processor, ScriptIndex.ScriptWithOneNugetDirectivePath, referencedScripts);

        await nugetHandler.Received(1).HandleNugetPackages(Arg.Any<List<NugetPackageDescription>>());
        processedContent.Should().NotContain(ParsingConstants.NugetDirective);
    }

    [Fact]
    public async Task ScriptWithMultipleNugetDirectivesShouldBeHandledProperly()
    {
        var nugetHandler = Substitute.For<INugetPackageHandler>();
        var processor = CreateScriptPreProcessor(nugetHandler);
        List<string> referencedScripts = [];

        var processedContent =
            await PreProcessScript(processor, ScriptIndex.ScriptWithMultipleNugetDirectivesPath, referencedScripts);

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
            await PreProcessScript(processor, ScriptIndex.ScriptWithMultipleLoadAndNugetDirectivesPath, referencedScripts);

        var expectedLoadDirectives =
            string.Join(Environment.NewLine, GetExpectedDirectives(scriptRelativePathsWithoutFileExtensions));

        referencedScripts.Should().HaveCount(numberOfLoadDirectives);
        processedContent.Should().Contain(expectedLoadDirectives);

        referencedScripts.Should().HaveCount(numberOfLoadDirectives);

        foreach (var path in scriptRelativePathsWithoutFileExtensions)
        {
            referencedScripts.Should()
                .Contain(Path.Combine(ScriptIndex.RootSubFolderPath, path + Constants.ScriptFileExtension));
        }

        await nugetHandler.Received(1).HandleNugetPackages(Arg.Any<List<NugetPackageDescription>>());
        processedContent.Should().NotContain(ParsingConstants.NugetDirective);
    }

    private async Task<string> PreProcessScript(ScriptPreProcessor processor, string scriptPath, List<string> referencedScripts)
        => await processor.ProcessScript(
            scriptPath,
            await File.ReadAllTextAsync(scriptPath),
            ScriptIndex.RootFolderPath,
            _tempFolderPath,
            referencedScripts);

    private List<string> GetExpectedDirectives(params string[] names)
    {
        List<string> list = [];
        var tmpBasePath = Path.Combine(_tempFolderPath, ScriptIndex.RootFolderName, ScriptIndex.RootSubFolder);

        for (var i = 0; i < names.Length; i++)
        {
            list.Add($"{ParsingConstants.LoadScriptDirective} " +
                $"\"{tmpBasePath}{Path.DirectorySeparatorChar}{names[i]}{Constants.ScriptFileExtension}\"");
        }

        return list;
    }

    private static INugetPackageHandler GetNugetHandler()
        => new NugetPackageHandler(Substitute.For<ILogger<NugetPackageHandler>>());

    private static ScriptPreProcessor CreateScriptPreProcessor(INugetPackageHandler? nugetPackageHandler = null)
    {
        var logger = Substitute.For<ILogger<ScriptPreProcessor>>();

        if (nugetPackageHandler is null)
        {
            return new(GetNugetHandler(), logger);
        }
        else
        {
            return new(nugetPackageHandler, logger);
        }
    }
}
