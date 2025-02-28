namespace TeaPie.Http.Parsing;

public sealed class TestDirectivePatternBuilder
{
    private const string Prefix = "TEST-";
    private readonly BaseDirectivePatternBuilder _baseBuilder;

    private TestDirectivePatternBuilder(string directiveName)
    {
        _baseBuilder = new BaseDirectivePatternBuilder(directiveName, Prefix);
    }

    public static TestDirectivePatternBuilder Create(string directiveName) => new(directiveName);

    public string Build() => _baseBuilder.Build();

    public TestDirectivePatternBuilder AddParameter(string pattern, string? parameterName = null)
    {
        _baseBuilder.AddParameter(pattern, parameterName);
        return this;
    }

    public TestDirectivePatternBuilder AddStringParameter(string? parameterName = null)
    {
        _baseBuilder.AddStringParameter(parameterName);
        return this;
    }

    public TestDirectivePatternBuilder AddBooleanParameter(string? parameterName = null)
    {
        _baseBuilder.AddBooleanParameter(parameterName);
        return this;
    }

    public TestDirectivePatternBuilder AddNumberParameter(string? parameterName = null)
    {
        _baseBuilder.AddNumberParameter(parameterName);
        return this;
    }

    public TestDirectivePatternBuilder AddStatusCodesParameter(string? parameterName = null)
    {
        _baseBuilder.AddStatusCodesParameter(parameterName);
        return this;
    }

    public TestDirectivePatternBuilder AddHeaderNameParameter(string? parameterName = null)
    {
        _baseBuilder.AddHeaderNameParameter(parameterName);
        return this;
    }

    public TestDirectivePatternBuilder AddDateTimeParameter(string? parameterName = null)
    {
        _baseBuilder.AddDateTimeParameter(parameterName);
        return this;
    }

    public TestDirectivePatternBuilder AddStringArrayParameter(string? parameterName = null)
    {
        _baseBuilder.AddStringArrayParameter(parameterName);
        return this;
    }

    public TestDirectivePatternBuilder AddBooleanArrayParameter(string? parameterName = null)
    {
        _baseBuilder.AddBooleanArrayParameter(parameterName);
        return this;
    }

    public TestDirectivePatternBuilder AddNumberArrayParameter(string? parameterName = null)
    {
        _baseBuilder.AddNumberArrayParameter(parameterName);
        return this;
    }

    public TestDirectivePatternBuilder AddTimeOnlyParameter(string? parameterName = null)
    {
        _baseBuilder.AddTimeOnlyParameter(parameterName);
        return this;
    }
}
