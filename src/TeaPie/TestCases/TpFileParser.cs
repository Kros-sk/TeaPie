namespace TeaPie.TestCases;

internal class TpFileParser
{
    private static readonly string[] LineSeparators = [Constants.WindowsEndOfLine, Constants.UnixEndOfLine];

    /// <summary>
    /// Parses the content of a <c>.tp</c> file. Results are stored in <paramref name="context"/>.
    /// </summary>
    /// <param name="context">
    /// Parsing context that provides the file content and fallback name,
    /// and receives the resulting <see cref="TpTestCaseDefinition"/> list.
    /// </param>
    public void Parse(TpParsingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.Content);
        ArgumentException.ThrowIfNullOrWhiteSpace(context.FallbackName);

        var lines = context.Content.Split(LineSeparators, StringSplitOptions.None);

        if (HasTestCaseMarker(lines))
        {
            ParseExplicitTestCases(lines, context);
        }
        else
        {
            context.AddDefinition(ParseImplicitTestCase(lines, context.FallbackName));
        }
    }

    private static bool HasTestCaseMarker(string[] lines)
        => lines.Any(IsTestCaseMarker);

    private static void ParseExplicitTestCases(string[] lines, TpParsingContext context)
    {
        int i = 0;

        while (i < lines.Length)
        {
            if (IsTestCaseMarker(lines[i]))
            {
                var name = ExtractName(lines[i], TpConstants.TestCaseMarker);
                i++;
                var (def, nextIndex) = ParseTestCaseBlock(lines, i, name);
                context.AddDefinition(def);
                i = nextIndex;
            }
            else
            {
                i++;
            }
        }
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

        return (string.Join(Constants.UnixEndOfLine, sectionLines).Trim(), i);
    }

    private static bool IsMarker(string line, string marker)
        => line.TrimStart().Equals(marker, StringComparison.OrdinalIgnoreCase);

    private static bool IsAnyMarker(string line)
        => IsTestCaseMarker(line)
            || IsMarker(line, TpConstants.InitMarker)
            || IsMarker(line, TpConstants.HttpMarker)
            || IsMarker(line, TpConstants.TestMarker)
            || IsMarker(line, TpConstants.EndMarker);

    private static bool IsTestCaseMarker(string line)
        => line.TrimStart().StartsWith(TpConstants.TestCaseMarker + " ", StringComparison.OrdinalIgnoreCase)
            || line.TrimStart().Equals(TpConstants.TestCaseMarker, StringComparison.OrdinalIgnoreCase);

    private static string ExtractName(string line, string marker)
    {
        var name = line.Substring(marker.Length).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException(
                $"A '{TpConstants.TestCaseMarker}' marker is missing its name. " +
                $"Expected format: '{TpConstants.TestCaseMarker} <Name>'");
        }

        return name;
    }
}
