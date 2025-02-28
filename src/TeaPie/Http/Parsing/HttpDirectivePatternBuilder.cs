using System.Text;

namespace TeaPie.Http.Parsing;

internal sealed class HttpDirectivePatternBuilder : BaseDirectivePatternBuilder
{
    private HttpDirectivePatternBuilder(string directiveName) : base(directiveName) { }

    public static HttpDirectivePatternBuilder Create(string directiveName) => new(directiveName);

    internal HttpDirectivePatternBuilder WithPrefix(string prefix)
    {
        _prefix = prefix;
        return this;
    }

    internal HttpDirectivePatternBuilder SetParameterSeparator(string separator)
    {
        _parameterSeparator = separator;
        return this;
    }

    public override string Build()
    {
        var regexBuilder = new StringBuilder();
        regexBuilder.Append(HttpFileParserConstants.DirectivePrefixPattern);
        regexBuilder.Append("(?<DirectiveName>" + _prefix + _directiveName + ")");

        if (_parameterPatterns.Count > 0)
        {
            regexBuilder.Append(DefaultSeparator);
            regexBuilder.AppendJoin(_parameterSeparator, _parameterPatterns);
        }

        regexBuilder.Append(@"\s*$");
        return regexBuilder.ToString();
    }
}
