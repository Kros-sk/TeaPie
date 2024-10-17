using FluentAssertions;
using System.Diagnostics.CodeAnalysis;
using TeaPie.ScriptHandling;

namespace TeaPie.Tests.ScriptHandling;

public sealed class ScriptPreProcessorShould : IDisposable
{
    private string? _tempDirectoryPath;
    private string? _scriptPath;

    [Fact]
    public async Task EmptyScriptFileShouldNotCauseProblem()
    {
        var processor = new ScriptPreProcessor();
        await CreateScriptFile(string.Empty);
        var processedContent = await processor.PrepareScript(_scriptPath, await File.ReadAllTextAsync(_scriptPath));

        processedContent.Should().BeEquivalentTo(string.Empty);
    }

    [Fact]
    public async Task ScriptWithoutAnyDirectivesShouldRemainTheSame()
    {
        var processor = new ScriptPreProcessor();
        const string code = @"Console.Writeline(""Hello World!"");";
        await CreateScriptFile(code);
        var processedContent = await processor.PrepareScript(_scriptPath, await File.ReadAllTextAsync(_scriptPath));

        processedContent.Should().BeEquivalentTo(code);
    }

    [MemberNotNull(nameof(_scriptPath))]
    private async Task CreateScriptFile(string content, string fileName = "")
    {
        if (_tempDirectoryPath is null || _tempDirectoryPath.Equals(string.Empty))
        {
            _tempDirectoryPath = Directory.CreateTempSubdirectory().FullName;
        }

        _scriptPath = Path.Combine(_tempDirectoryPath, fileName.Equals(string.Empty) ? "script.csx" : fileName);
        await File.WriteAllTextAsync(_scriptPath, content);
    }

    public void Dispose()
    {
        if (!string.IsNullOrEmpty(_tempDirectoryPath) && Directory.Exists(_tempDirectoryPath))
        {
            Directory.Delete(_tempDirectoryPath, true);
        }
    }
}
