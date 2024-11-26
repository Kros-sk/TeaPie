using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Exceptions;
using TeaPie.Scripts;

namespace TeaPie.Tests.Scripts;

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
            ScriptIndex.RootFolderFullPath,
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
                .Contain(Path.Combine(ScriptIndex.RootSubFolderFullPath, path + Constants.ScriptFileExtension));
        }

        processedContent.Should().Contain(expectedDirectives);
    }

    [Fact]
    public async Task ScriptWithInvalidNuGetDirectiveShouldThrowException()
    {
        var nugetHandler = GetNuGetHandler();

        var processor = CreateScriptPreProcessor(nugetHandler);

        List<string> referencedScripts = [];

        await processor.Invoking(async processor => await processor.ProcessScript(
            ScriptIndex.ScriptWithInvalidNuGetDirectivePath,
            await File.ReadAllTextAsync(ScriptIndex.ScriptWithInvalidNuGetDirectivePath),
            ScriptIndex.RootFolderFullPath,
            _tempFolderPath,
            referencedScripts))
            .Should().ThrowAsync<NuGetPackageNotFoundException>();
    }

    [Fact]
    public async Task ScriptWithOneNuGetDirectiveShouldBeHandledProperly()
    {
        var nugetHandler = Substitute.For<INuGetPackageHandler>();
        var processor = CreateScriptPreProcessor(nugetHandler);
        List<string> referencedScripts = [];

        var processedContent = await PreProcessScript(processor, ScriptIndex.ScriptWithOneNuGetDirectivePath, referencedScripts);

        await nugetHandler.Received(1).HandleNuGetPackages(Arg.Any<List<NuGetPackageDescription>>());
        processedContent.Should().NotContain(ScriptPreProcessorConstants.NuGetDirective);
    }

    [Fact]
    public async Task ScriptWithMultipleNuGetDirectivesShouldBeHandledProperly()
    {
        var nugetHandler = Substitute.For<INuGetPackageHandler>();
        var processor = CreateScriptPreProcessor(nugetHandler);
        List<string> referencedScripts = [];

        var processedContent =
            await PreProcessScript(processor, ScriptIndex.ScriptWithMultipleNuGetDirectivesPath, referencedScripts);

        await nugetHandler.Received(1).HandleNuGetPackages(Arg.Any<List<NuGetPackageDescription>>());
        processedContent.Should().NotContain(ScriptPreProcessorConstants.NuGetDirective);
    }

    [Fact]
    public async Task ScriptWithDuplicatedNuGetDirectivesShouldBeHandledProperly()
    {
        var nugetHandler = Substitute.For<INuGetPackageHandler>();
        var processor = CreateScriptPreProcessor(nugetHandler);
        List<string> referencedScripts = [];

        var processedContent =
            await PreProcessScript(processor, ScriptIndex.ScriptWithDuplicatedNuGetDirectivePath, referencedScripts);

        await nugetHandler.Received(1).HandleNuGetPackages(Arg.Any<List<NuGetPackageDescription>>());
        processedContent.Should().NotContain(ScriptPreProcessorConstants.NuGetDirective);
    }

    [Fact]
    public async Task ScriptWithMultipleLoadAndNuGetDirectivesShouldBeHandledProperly()
    {
        var nugetHandler = Substitute.For<INuGetPackageHandler>();
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
            await PreProcessScript(processor, ScriptIndex.ScriptWithMultipleLoadAndNuGetDirectivesPath, referencedScripts);

        var expectedLoadDirectives =
            string.Join(Environment.NewLine, GetExpectedDirectives(scriptRelativePathsWithoutFileExtensions));

        referencedScripts.Should().HaveCount(numberOfLoadDirectives);
        processedContent.Should().Contain(expectedLoadDirectives);

        referencedScripts.Should().HaveCount(numberOfLoadDirectives);

        foreach (var path in scriptRelativePathsWithoutFileExtensions)
        {
            referencedScripts.Should()
                .Contain(Path.Combine(ScriptIndex.RootSubFolderFullPath, path + Constants.ScriptFileExtension));
        }

        await nugetHandler.Received(1).HandleNuGetPackages(Arg.Any<List<NuGetPackageDescription>>());
        processedContent.Should().NotContain(ScriptPreProcessorConstants.NuGetDirective);
    }

    private async Task<string> PreProcessScript(ScriptPreProcessor processor, string scriptPath, List<string> referencedScripts)
        => await processor.ProcessScript(
            scriptPath,
            await File.ReadAllTextAsync(scriptPath),
            ScriptIndex.RootFolderFullPath,
            _tempFolderPath,
            referencedScripts);

    private List<string> GetExpectedDirectives(params string[] names)
    {
        List<string> list = [];
        var tmpBasePath = Path.Combine(_tempFolderPath, ScriptIndex.RootFolderName, ScriptIndex.RootSubFolderName);

        for (var i = 0; i < names.Length; i++)
        {
            list.Add($"{ScriptPreProcessorConstants.LoadScriptDirective} " +
                $"\"{tmpBasePath}{Path.DirectorySeparatorChar}{names[i]}{Constants.ScriptFileExtension}\"");
        }

        return list;
    }

    private static INuGetPackageHandler GetNuGetHandler()
        => new NuGetPackageHandler(Substitute.For<ILogger<NuGetPackageHandler>>(), NuGet.Common.NullLogger.Instance);

    private static ScriptPreProcessor CreateScriptPreProcessor(INuGetPackageHandler? nugetPackageHandler = null)
    {
        var logger = Substitute.For<ILogger<ScriptPreProcessor>>();

        if (nugetPackageHandler is null)
        {
            return new(GetNuGetHandler(), logger);
        }
        else
        {
            return new(nugetPackageHandler, logger);
        }
    }
}
