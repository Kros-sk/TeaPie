using FluentAssertions;
using NSubstitute;
using System.Diagnostics.CodeAnalysis;
using TeaPie.Exceptions;
using TeaPie.Parsing;
using TeaPie.ScriptHandling;

namespace TeaPie.Tests.ScriptHandling;

public sealed class ScriptPreProcessorShould
{
    private const string ScriptCode = "Console.WriteLine(\"Script executed!\");";

    private const string ReferencedScriptLoadDirective =
        ParsingConstants.ReferenceScriptDirective + "\"..\\" + ReferencedScriptRelativePath + "\"";

    private const string NugetPackageDirective = $"{ParsingConstants.NugetDirective} \"Newtonsoft.Json, 13.0.3\"";

    private const string RootFolderName = "root";

    private const string ScriptRelativePath = "script" + Constants.ScriptFileExtension;
    private const string ReferencedScriptRelativePath = "referencedScript" + Constants.ScriptFileExtension;

    private readonly string _tempDestinationFolderPath = Path.Combine(Path.GetTempPath(), "destination");
    private string? _tempSourceFolderPath;
    private string? _scriptPath;

    [Fact]
    public async Task EmptyScriptFileShouldNotCauseProblem()
    {
        var processor = CreateScriptPreProcessor();
        SetPaths();
        List<string> referencedScripts = [];
        var processedContent = await processor.PrepareScript(
            _scriptPath,
            string.Empty,
            _tempSourceFolderPath,
            _tempDestinationFolderPath,
            referencedScripts);

        processedContent.Should().BeEquivalentTo(string.Empty);
    }

    private static ScriptPreProcessor CreateScriptPreProcessor(INugetPackageHandler? nugetPackageHandler = null)
        => nugetPackageHandler is null
            ? new(Substitute.For<INugetPackageHandler>())
            : new(nugetPackageHandler);

    [Fact]
    public async Task ScriptWithoutAnyDirectivesShouldRemainTheSame()
    {
        var processor = CreateScriptPreProcessor();
        SetPaths();
        List<string> referencedScripts = [];
        var processedContent = await processor.PrepareScript(
            _scriptPath,
            ScriptCode,
            _tempSourceFolderPath,
            _tempDestinationFolderPath,
            referencedScripts);

        processedContent.Should().BeEquivalentTo(ScriptCode);
    }

    [Fact]
    public async Task ScriptWithOneLoadDirectiveShouldBeResolvedCorrectly()
    {
        var processor = CreateScriptPreProcessor();
        var directive = ReferencedScriptLoadDirective + Environment.NewLine;
        var codeWithoutDirective = ScriptCode + Environment.NewLine;

        var code = directive + codeWithoutDirective;

        SetPaths();
        List<string> referencedScripts = [];
        var processedContent = await processor.PrepareScript(
            _scriptPath,
            code,
            _tempSourceFolderPath,
            _tempDestinationFolderPath,
            referencedScripts);

        var expectedDirective = $"{ParsingConstants.ReferenceScriptDirective} " +
            $"\"{Path.Combine(_tempDestinationFolderPath, RootFolderName, ReferencedScriptRelativePath)}\"" +
            $"{Environment.NewLine}";

        referencedScripts.Should().HaveCount(1);

        processedContent.Should().BeEquivalentTo(expectedDirective + codeWithoutDirective);
    }

    [Fact]
    public async Task ScriptWithMultipleLoadDirectivesShouldBeResolvedCorrectly()
    {
        var processor = CreateScriptPreProcessor();
        const int numberOfDirectives = 10;
        var loadDirectives = new string[numberOfDirectives];
        var fileNames = new string[numberOfDirectives];
        var loadDirectivesCode = string.Empty;

        for (var i = 0; i < loadDirectives.Length; i++)
        {
            fileNames[i] = Path.GetRandomFileName() + Constants.ScriptFileExtension;
            loadDirectives[i] = $"{ParsingConstants.ReferenceScriptDirective} \"..\\{fileNames[i]}\"";
            loadDirectivesCode += loadDirectives[i] + Environment.NewLine;
        }

        var code = loadDirectivesCode + ScriptCode;

        SetPaths();
        List<string> referencedScripts = [];
        var processedContent = await processor.PrepareScript(
            _scriptPath,
            code,
            _tempSourceFolderPath,
            _tempDestinationFolderPath,
            referencedScripts);

        var expectedLoadDirectives = new string[numberOfDirectives];
        var expectedLoadDirectivesCode = string.Empty;

        for (var i = 0; i < loadDirectives.Length; i++)
        {
            expectedLoadDirectives[i] = $"{ParsingConstants.ReferenceScriptDirective} " +
                $"\"{Path.Combine(_tempDestinationFolderPath, RootFolderName, fileNames[i])}\"";
            expectedLoadDirectivesCode += expectedLoadDirectives[i] + Environment.NewLine;
        }

        referencedScripts.Should().HaveCount(numberOfDirectives);
        processedContent.Should().BeEquivalentTo(expectedLoadDirectivesCode + ScriptCode);
    }

    [Fact]
    public async Task ScriptWithInvalidNugetDirectiveShouldBeHandledProperly()
    {
        var nugetHandler = new NugetPackageHandler();
        var processor = CreateScriptPreProcessor(nugetHandler);
        var code = $"{ParsingConstants.NugetDirective} \"{Guid.NewGuid()}, 22.0.2\"";

        SetPaths();
        List<string> referencedScripts = [];

        await processor.Invoking(async processor => await processor.PrepareScript(
            _scriptPath,
            code,
            _tempSourceFolderPath,
            _tempDestinationFolderPath,
            referencedScripts))
            .Should().ThrowAsync<NugetPackageNotFoundException>();
    }

    [Fact]
    public async Task ScriptWithOneNugetDirectiveShouldBeHandledProperly()
    {
        var nugetHandler = Substitute.For<INugetPackageHandler>();
        var processor = CreateScriptPreProcessor(nugetHandler);
        var code = NugetPackageDirective + Environment.NewLine + ScriptCode;
        List<string> referencedScripts = [];

        SetPaths();

        await processor.PrepareScript(
            _scriptPath,
            code,
            _tempSourceFolderPath,
            _tempDestinationFolderPath,
            referencedScripts);

        await nugetHandler.Received(1).HandleNugetPackages(Arg.Any<List<NugetPackageDescription>>());
    }

    [MemberNotNull(nameof(_scriptPath))]
    [MemberNotNull(nameof(_tempSourceFolderPath))]
    private void SetPaths()
    {
        if (_tempSourceFolderPath?.Equals(string.Empty) != false)
        {
            var tmpFolder = Path.GetRandomFileName();
            _tempSourceFolderPath = Path.Combine(Path.GetTempFileName(), tmpFolder, RootFolderName);
        }

        _scriptPath = Path.Combine(_tempSourceFolderPath, ScriptRelativePath);
    }
}
