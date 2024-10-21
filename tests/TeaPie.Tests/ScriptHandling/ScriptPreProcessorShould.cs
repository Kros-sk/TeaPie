using FluentAssertions;
using System.Diagnostics.CodeAnalysis;
using TeaPie.ScriptHandling;

namespace TeaPie.Tests.ScriptHandling;

public sealed class ScriptPreProcessorShould : IDisposable
{
    private const string ScriptCode = """
Console.WriteLine("Script executed!");
""";

    private const string ReferencedScriptLoadDirective = """
#load "..\referencedScript.csx"
""";

    private const string RootFolderName = "root";

    private const string ScriptRelativePath = "script.csx";
    private const string ReferencedScriptRelativePath = "referencedScript.csx";

    private readonly string _tempDestinationFolderPath = Path.Combine(Path.GetTempPath(), "destination");
    private string? _tempSourceFolderPath;
    private string? _scriptPath;

    [Fact]
    public async Task EmptyScriptFileShouldNotCauseProblem()
    {
        var processor = new ScriptPreProcessor();
        await CreateScriptFile(string.Empty);
        List<string> referencedScripts = [];
        var processedContent = await processor.PrepareScript(
            _scriptPath,
            await File.ReadAllTextAsync(_scriptPath),
            _tempSourceFolderPath,
            _tempDestinationFolderPath,
            referencedScripts);

        processedContent.Should().BeEquivalentTo(string.Empty);
    }

    [Fact]
    public async Task ScriptWithoutAnyDirectivesShouldRemainTheSame()
    {
        var processor = new ScriptPreProcessor();
        await CreateScriptFile(ScriptCode);
        List<string> referencedScripts = [];
        var processedContent = await processor.PrepareScript(
            _scriptPath,
            await File.ReadAllTextAsync(_scriptPath),
            _tempSourceFolderPath,
            _tempDestinationFolderPath,
            referencedScripts);

        processedContent.Should().BeEquivalentTo(ScriptCode);
    }

    [Fact]
    public async Task ScriptWithOneDirectiveShouldBeResolvedCorrectly()
    {
        var processor = new ScriptPreProcessor();
        var directive = ReferencedScriptLoadDirective + Environment.NewLine;
        var codeWithoutDirective = ScriptCode + Environment.NewLine;

        var code = directive + codeWithoutDirective;

        await CreateScriptFile(code);
        List<string> referencedScripts = [];
        var processedContent = await processor.PrepareScript(
            _scriptPath,
            await File.ReadAllTextAsync(_scriptPath),
            _tempSourceFolderPath,
            _tempDestinationFolderPath,
            referencedScripts);

        var expectedDirective = $"{Constants.ReferenceScriptDirective} " +
            $"\"{Path.Combine(_tempDestinationFolderPath, RootFolderName, ReferencedScriptRelativePath)}\"" +
            $"{Environment.NewLine}";

        referencedScripts.Should().HaveCount(1);

        processedContent.Should().BeEquivalentTo(expectedDirective + codeWithoutDirective);
    }

    [Fact]
    public async Task ScriptWithMultipleDirectivesShouldBeResolvedCorrectly()
    {
        var processor = new ScriptPreProcessor();
        const int numberOfDirectives = 10;
        var loadDirectives = new string[numberOfDirectives];
        var fileNames = new string[numberOfDirectives];
        var loadDirectivesCode = string.Empty;

        for (var i = 0; i < loadDirectives.Length; i++)
        {
            fileNames[i] = Path.GetRandomFileName() + Constants.ScriptFileExtension;
            loadDirectives[i] = $"{Constants.ReferenceScriptDirective} \"..\\{fileNames[i]}\"";
            loadDirectivesCode += loadDirectives[i] + Environment.NewLine;
        }

        var code = loadDirectivesCode + ScriptCode;

        await CreateScriptFile(code);
        List<string> referencedScripts = [];
        var processedContent = await processor.PrepareScript(
            _scriptPath,
            await File.ReadAllTextAsync(_scriptPath),
            _tempSourceFolderPath,
            _tempDestinationFolderPath,
            referencedScripts);

        var expectedLoadDirectives = new string[numberOfDirectives];
        var expectedLoadDirectivesCode = string.Empty;

        for (var i = 0; i < loadDirectives.Length; i++)
        {
            expectedLoadDirectives[i] = $"{Constants.ReferenceScriptDirective} " +
                $"\"{Path.Combine(_tempDestinationFolderPath, RootFolderName, fileNames[i])}\"";
            expectedLoadDirectivesCode += expectedLoadDirectives[i] + Environment.NewLine;
        }

        referencedScripts.Should().HaveCount(numberOfDirectives);

        processedContent.Should().BeEquivalentTo(expectedLoadDirectivesCode + ScriptCode);
    }

    /// <summary>
    /// If doesn't exist, creates temporary folder with randomly generated name, to which root folder is placed. The script
    /// with the specified name is then created within root folder.
    /// </summary>
    /// <param name="content">Content of the script file.</param>
    /// <returns>Task - asynchronous method.</returns>
    [MemberNotNull(nameof(_scriptPath))]
    [MemberNotNull(nameof(_tempSourceFolderPath))]
    private async Task CreateScriptFile(string content)
    {
        if (_tempSourceFolderPath?.Equals(string.Empty) != false)
        {
            var tmpFolder = Directory.CreateTempSubdirectory().FullName;
            _tempSourceFolderPath = Path.Combine(tmpFolder, RootFolderName);
            Directory.CreateDirectory(_tempSourceFolderPath);
        }

        _scriptPath = Path.Combine(_tempSourceFolderPath, ScriptRelativePath);
        await File.WriteAllTextAsync(_scriptPath, content);
    }

    public void Dispose()
    {
        if (!string.IsNullOrEmpty(_tempSourceFolderPath) && Directory.Exists(_tempSourceFolderPath))
        {
            Directory.Delete(_tempSourceFolderPath, true);
        }
    }
}
