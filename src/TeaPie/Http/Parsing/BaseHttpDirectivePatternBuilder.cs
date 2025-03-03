using System.Text;

namespace TeaPie.Http.Parsing;

internal class BaseDirectivePatternBuilder(string directiveName, string prefix = "", string parameterSeparator = @"\s*;\s*")
{
    protected readonly string _directiveName = directiveName;
    protected readonly List<string> _parameterPatterns = [];
    protected string _prefix = prefix;
    protected string _parameterSeparator = parameterSeparator;

    internal const string DefaultSeparator = @"\s*:\s*";
    internal const string StringParameterPattern = @".+?";
    internal const string StatusCodesParameterPattern = @"\[(\d+,\s*)*\d+\]";
    internal const string HeaderNameParameterPattern = "[A-Za-z0-9!#$%&'*+.^_`|~-]+";
    internal const string NumberParameterPattern = @"\d+";
    internal const string BoolParameterPattern = "(?i)true|false";
    internal const string DateTimeParameterPattern =
        @"\d{4}-\d{2}-\d{2}|\d{2}/\d{2}/\d{4}|\d{2}\.\d{2}\.\d{4}(?:\s\d{2}:\d{2}:\d{2})?";
    internal const string StringArrayPattern = @"\[\s*(?:""[^""]+""\s*,\s*)*""[^""]+""\s*\]";
    internal const string NumberArrayPattern = @"\[\s*(?:\d+\s*,\s*)*\d+\s*\]";
    internal const string BoolArrayPattern = @"\[\s*(?:(?i)true|false\s*,\s*)*(?i)true|false\s*\]";
    internal const string TimeOnlyParameterPattern = @"\d{2}:\d{2}:\d{2}(?:\.\d{1,3})?";

    public virtual string Build()
    {
        var regexBuilder = new StringBuilder();
        regexBuilder.Append(HttpFileParserConstants.DirectivePrefixPattern);
        regexBuilder.Append(@"(?<DirectiveName>" + _prefix + _directiveName + @")");

        if (_parameterPatterns.Count > 0)
        {
            regexBuilder.Append(DefaultSeparator);
            regexBuilder.AppendJoin(_parameterSeparator, _parameterPatterns);
        }

        regexBuilder.Append(@"\s*$");
        return regexBuilder.ToString();
    }

    public BaseDirectivePatternBuilder AddParameter(string pattern, string? parameterName = null)
    {
        parameterName ??= $"Parameter{_parameterPatterns.Count + 1}";
        _parameterPatterns.Add($"(?<{parameterName}>{pattern})");
        return this;
    }

    public BaseDirectivePatternBuilder AddStringParameter(string? parameterName = null)
        => AddParameter(StringParameterPattern, parameterName);

    public BaseDirectivePatternBuilder AddBooleanParameter(string? parameterName = null)
        => AddParameter(BoolParameterPattern, parameterName);

    public BaseDirectivePatternBuilder AddNumberParameter(string? parameterName = null)
        => AddParameter(NumberParameterPattern, parameterName);

    public BaseDirectivePatternBuilder AddStatusCodesParameter(string? parameterName = null)
        => AddParameter(StatusCodesParameterPattern, parameterName);

    public BaseDirectivePatternBuilder AddHeaderNameParameter(string? parameterName = null)
        => AddParameter(HeaderNameParameterPattern, parameterName);

    public BaseDirectivePatternBuilder AddDateTimeParameter(string? parameterName = null)
        => AddParameter(DateTimeParameterPattern, parameterName);

    public BaseDirectivePatternBuilder AddStringArrayParameter(string? parameterName = null)
        => AddParameter(StringArrayPattern, parameterName);

    public BaseDirectivePatternBuilder AddBooleanArrayParameter(string? parameterName = null)
        => AddParameter(BoolArrayPattern, parameterName);

    public BaseDirectivePatternBuilder AddNumberArrayParameter(string? parameterName = null)
        => AddParameter(NumberArrayPattern, parameterName);

    public BaseDirectivePatternBuilder AddTimeOnlyParameter(string? parameterName = null)
        => AddParameter(TimeOnlyParameterPattern, parameterName);
}
