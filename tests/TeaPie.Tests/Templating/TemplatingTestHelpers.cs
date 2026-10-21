using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TeaPie.Templating;

namespace TeaPie.Tests.Templating;

internal static partial class TemplatingTestHelpers
{
    public static TemplateExpander CreateExpander(
        global::TeaPie.Variables.IVariables? variables = null, TemplatingLimits? limits = null,
        ILogger<TemplateExpander>? logger = null)
    {
        var vars = variables ?? new global::TeaPie.Variables.Variables();
        return new TemplateExpander(
            new LoopBlockScanner(),
            new LoopBodyMasker(),
            new CollectionSourceResolver(vars),
            new VariablesFluidModelBuilder(),
            vars,
            limits ?? new TemplatingLimits(),
            logger ?? NullLogger<TemplateExpander>.Instance);
    }

    public static (int Line, int Column) ExtractReportedPosition(string message)
    {
        var match = PositionRegex().Match(message);
        match.Success.Should().BeTrue($"expected message to contain a '(line:column)' position: '{message}'");
        return (int.Parse(match.Groups["line"].Value), int.Parse(match.Groups["column"].Value));
    }

    public static (int Line, int Column) FindOriginalPosition(string content, string substring)
    {
        var index = content.IndexOf(substring, StringComparison.Ordinal);
        index.Should().BeGreaterOrEqualTo(0, $"'{substring}' should be present in the original content");

        var line = 1;
        var lineStart = 0;
        for (var i = 0; i < index; i++)
        {
            if (content[i] == '\n')
            {
                line++;
                lineStart = i + 1;
            }
        }

        return (line, index - lineStart + 1);
    }

    [GeneratedRegex(@"at \((?<line>\d+):(?<column>\d+)\)")]
    private static partial Regex PositionRegex();
}

internal sealed class RecordingLogger<T> : ILogger<T>
{
    public List<(LogLevel Level, string Message)> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => Entries.Add((logLevel, formatter(state, exception)));
}
