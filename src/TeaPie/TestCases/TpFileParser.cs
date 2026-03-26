using System.Text.RegularExpressions;

namespace TeaPie.TestCases;

internal class TpFileParser
{
    private static readonly string[] LineSeparators = [Constants.WindowsEndOfLine, Constants.UnixEndOfLine];

    /// <summary>
    /// Parses the content of a <c>.tp</c> file and stores the resulting
    /// test case definitions in <see cref="TpParsingContext.Definitions"/>.
    /// </summary>
    /// <param name="context">
    /// Parsing context that carries the input content and receives the parsed definitions.
    /// </param>
    public void Parse(TpParsingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var lines = context.Content.Split(LineSeparators, StringSplitOptions.None);

        var definitions = HasTestCaseMarker(lines)
            ? ParseExplicitTestCases(lines, context.FallbackName)
            : [ParseImplicitTestCase(lines, context.FallbackName)];

        context.Definitions.AddRange(definitions);
    }

    private static bool HasTestCaseMarker(string[] lines)
        => lines.Any(IsTestCaseMarker);

    private static List<TpTestCaseDefinition> ParseExplicitTestCases(string[] lines, string fallbackName)
    {
        var definitions = new List<TpTestCaseDefinition>();
        int i = 0;

        while (i < lines.Length)
        {
            if (IsTestCaseMarker(lines[i]))
            {
                var name = ExtractNameOrDefault(lines[i], TpConstants.TestCaseMarker, fallbackName);
                i++;
                var (def, nextIndex) = ParseTestCaseBlock(lines, i, name);
                definitions.Add(def);
                i = nextIndex;
            }
            else
            {
                i++;
            }
        }

        if (definitions.Count > 1 && definitions.Any(d => d.Name == fallbackName))
        {
            throw new InvalidOperationException(
                $"A '{TpConstants.TestCaseMarker}' marker is missing its name. " +
                "Omitting the name is only allowed when the file contains a single test case. " +
                $"Expected format: '{TpConstants.TestCaseMarker} <Name>'");
        }

        return definitions;
    }

    private static TpTestCaseDefinition ParseImplicitTestCase(string[] lines, string fallbackName)
    {
        var (def, _) = ParseTestCaseBlock(lines, 0, fallbackName);
        return def;
    }

    private static (TpTestCaseDefinition Definition, int NextIndex) ParseTestCaseBlock(
        string[] lines, int startIndex, string name)
    {
        string? initContent = null;
        string? httpContent = null;
        string? testContent = null;

        var i = startIndex;
        while (i < lines.Length)
        {
            var line = lines[i];

            if (IsMarker(line, TpConstants.EndMarker))
            {
                i++;
                break;
            }

            if (IsTestCaseMarker(line))
            {
                break;
            }

            if (IsMarker(line, TpConstants.InitMarker))
            {
                i++;
                (initContent, i) = ExtractSectionContent(lines, i);
            }
            else if (IsMarker(line, TpConstants.HttpMarker))
            {
                i++;
                (httpContent, i) = ExtractSectionContent(lines, i);
            }
            else if (IsMarker(line, TpConstants.TestMarker))
            {
                i++;
                (testContent, i) = ExtractSectionContent(lines, i);
            }
            else
            {
                i++;
            }
        }

        if (httpContent is null)
        {
            throw new InvalidOperationException(
                $"Test case '{name}' in .tp file is missing the required '{TpConstants.HttpMarker}' section.");
        }

        return (new TpTestCaseDefinition(name, initContent, httpContent, testContent), i);
    }

    private static (string Content, int NextIndex) ExtractSectionContent(string[] lines, int startIndex)
    {
        var sectionLines = new List<string>();
        var i = startIndex;

        while (i < lines.Length)
        {
            var line = lines[i];

            if (IsAnyMarker(line))
            {
                break;
            }

            sectionLines.Add(line);
            i++;
        }

        return (string.Join("\n", sectionLines).Trim(), i);
    }

    private static readonly Regex MarkerNormalizer = new(@"^---\s*", RegexOptions.Compiled);

    private static string NormalizeLine(string line)
        => MarkerNormalizer.Replace(line.TrimStart(), "--- ", 1);

    private static bool IsMarker(string line, string marker)
        => NormalizeLine(line).Equals(marker, StringComparison.OrdinalIgnoreCase);

    private static bool IsAnyMarker(string line)
        => IsTestCaseMarker(line)
            || IsMarker(line, TpConstants.InitMarker)
            || IsMarker(line, TpConstants.HttpMarker)
            || IsMarker(line, TpConstants.TestMarker)
            || IsMarker(line, TpConstants.EndMarker);

    private static bool IsTestCaseMarker(string line)
    {
        var normalized = NormalizeLine(line);
        return normalized.StartsWith(TpConstants.TestCaseMarker + " ", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals(TpConstants.TestCaseMarker, StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractNameOrDefault(string line, string marker, string fallbackName)
    {
        var normalized = NormalizeLine(line);
        var name = normalized.Length > marker.Length ? normalized[marker.Length..].Trim() : string.Empty;
        return string.IsNullOrWhiteSpace(name) ? fallbackName : name;
    }
}
