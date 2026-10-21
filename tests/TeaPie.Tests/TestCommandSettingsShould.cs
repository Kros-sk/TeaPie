using FluentAssertions;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using TeaPie.DotnetTool;

namespace TeaPie.Tests;

public class TestCommandSettingsShould
{
    [Theory]
    [InlineData(new[] { "test", "--dump-expanded", "./coll" }, true, null)]
    [InlineData(new[] { "test", "./coll", "--dump-expanded" }, true, null)]
    [InlineData(new[] { "test", "--dump-expanded-dir", "./out", "./coll" }, true, "./out")]
    [InlineData(new[] { "test", "./coll", "--dump-expanded-dir", "./out" }, true, "./out")]
    [InlineData(new[] { "test", "--dump-expanded", "--dump-expanded-dir", "./out", "./coll" }, true, "./out")]
    [InlineData(new[] { "test", "./coll" }, false, null)]
    public void KeepThePositionalPathSeparateFromTheDumpOptions(
        string[] args, bool expectedDump, string? expectedDirectory)
    {
        var app = new CommandAppTester();
        app.Configure(config => config.AddCommand<SettingsCapturingCommand>("test"));

        var result = app.Run(args);

        result.ExitCode.Should().Be(0);
        var settings = result.Settings.Should().BeOfType<TestCommand.Settings>().Subject;
        settings.Path.Should().Be("./coll");
        settings.ShouldDumpExpanded.Should().Be(expectedDump);
        settings.DumpExpandedDirectory.Should().Be(expectedDirectory);
    }

    private sealed class SettingsCapturingCommand : Command<TestCommand.Settings>
    {
        public override int Execute(CommandContext context, TestCommand.Settings settings) => 0;
    }
}
