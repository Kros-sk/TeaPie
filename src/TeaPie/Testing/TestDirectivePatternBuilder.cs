using System.Text;
using TeaPie.Http.Parsing;

namespace TeaPie.Testing;

internal sealed class TestDirectivePatternBuilder
{
    private const string DirectivePrefixPattern =
        HttpFileParserConstants.DirectivePrefixPattern + HttpFileParserConstants.TestDirectivePrefix;

    private const string DefaultSeparator = @"\s*:\s*";

    private readonly string _directiveName;
    private string _parameterSeparator = @"\s*;\s*";
    private readonly List<string> _parameterPatterns = [];
    private const string StringParameterPattern = @"\S+";
    private const string StatusCodesParameterPattern = @"\[(\d+,\s*)*\d+\]";
    private const string HeaderNameParameterPattern = "[A-Za-z0-9!#$%&'*+.^_`|~-]+";
    private const string NumberParameterPattern = @"\d+";
    private const string BoolParameterPattern = "(?i)true|false";
    private const string DateTimeParameterParameter =
        @"\d{4}-\d{2}-\d{2}|\d{2}/\d{2}/\d{4}|\d{2}\.\d{2}\.\d{4}(?:\s\d{2}:\d{2}:\d{2})?";
    private const string StringArrayPattern = @"\[\s*(?:""[^""]+""\s*,\s*)*""[^""]+""\s*\]";
    private const string NumberArrayPattern = @"\[\s*(?:\d+\s*,\s*)*\d+\s*\]";
    private const string BoolArrayPattern = @"\[\s*(?:(?i)true|false\s*,\s*)*(?i)true|false\s*\]";

    private TestDirectivePatternBuilder(string directiveName) => _directiveName = directiveName;

    public static TestDirectivePatternBuilder Create(string directiveName) => new(directiveName);

    public TestDirectivePatternBuilder SetParameterSeparator(string separator)
    {
        _parameterSeparator = separator;
        return this;
    }

    public TestDirectivePatternBuilder AddParameter(string pattern)
    {
        var parameterName = $"Parameter{_parameterPatterns.Count + 1}";
        _parameterPatterns.Add($"(?<{parameterName}>{pattern})");
        return this;
    }

    public string Build()
    {
        var regexBuilder = new StringBuilder();
        regexBuilder.Append(DirectivePrefixPattern);
        regexBuilder.Append(@"(?<DirectiveName>" + _directiveName + @")");

        if (_parameterPatterns.Count > 0)
        {
            regexBuilder.Append(DefaultSeparator);
            regexBuilder.AppendJoin(_parameterSeparator, _parameterPatterns);
        }

        regexBuilder.Append(@"\s*$");
        return regexBuilder.ToString();
    }

    public TestDirectivePatternBuilder AddStringParameter()
        => AddParameter(StringParameterPattern);

    public TestDirectivePatternBuilder AddBooleanParameter()
        => AddParameter(BoolParameterPattern);

    public TestDirectivePatternBuilder AddNumberParameter()
        => AddParameter(NumberParameterPattern);

    public TestDirectivePatternBuilder AddStatusCodesParameter()
        => AddParameter(StatusCodesParameterPattern);

    public TestDirectivePatternBuilder AddHeaderNameParameter()
        => AddParameter(HeaderNameParameterPattern);

    public TestDirectivePatternBuilder AddDateTimeParameter()
        => AddParameter(DateTimeParameterParameter);

    public TestDirectivePatternBuilder AddStringArrayParameter()
        => AddParameter(StringArrayPattern);

    public TestDirectivePatternBuilder AddBooleanArrayParameter()
        => AddParameter(BoolArrayPattern);

    public TestDirectivePatternBuilder AddNumberArrayParameter()
        => AddParameter(NumberArrayPattern);
}
