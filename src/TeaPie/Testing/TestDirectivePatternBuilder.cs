using TeaPie.Http.Parsing;

namespace TeaPie.Testing;

/// <summary>
/// A builder for constructing regular expression patterns for test directives used in '.http' files.
/// </summary>
public sealed class TestDirectivePatternBuilder
{
    private const string Prefix = "TEST-";
    private readonly BaseDirectivePatternBuilder _baseBuilder;

    private TestDirectivePatternBuilder(string directiveName)
    {
        _baseBuilder = new BaseDirectivePatternBuilder(directiveName, Prefix);
    }

    /// <summary>
    /// Creates a new instance of <see cref="TestDirectivePatternBuilder"/>.
    /// </summary>
    /// <param name="directiveName">The name of the directive.</param>
    /// <returns>A new <see cref="TestDirectivePatternBuilder"/> instance.</returns>
    public static TestDirectivePatternBuilder Create(string directiveName) => new(directiveName);

    /// <summary>
    /// Builds the final directive pattern as a <see cref="string"/>.
    /// </summary>
    /// <returns>The constructed directive pattern.</returns>
    public string Build() => _baseBuilder.Build();

    /// <summary>
    /// Adds a custom parameter with a specified pattern.
    /// </summary>
    /// <param name="pattern">The regex pattern for the parameter.</param>
    /// <param name="parameterName">The optional name of the parameter. If not provided,
    /// a default name ("Parameter" + index, starting from 1) is assigned (e.g., "Parameter3").</param>
    /// <returns>The updated <see cref="TestDirectivePatternBuilder"/> instance.</returns>
    public TestDirectivePatternBuilder AddParameter(string pattern, string? parameterName = null)
    {
        _baseBuilder.AddParameter(pattern, parameterName);
        return this;
    }

    /// <summary>
    /// Adds a <see cref="string"/> parameter.
    /// </summary>
    /// <inheritdoc cref="AddParameter"/>
    public TestDirectivePatternBuilder AddStringParameter(string? parameterName = null)
    {
        _baseBuilder.AddStringParameter(parameterName);
        return this;
    }

    /// <summary>
    /// Adds a <see cref="bool"/> parameter.
    /// </summary>
    /// <inheritdoc cref="AddParameter"/>
    public TestDirectivePatternBuilder AddBooleanParameter(string? parameterName = null)
    {
        _baseBuilder.AddBooleanParameter(parameterName);
        return this;
    }

    /// <summary>
    /// Adds a numeric parameter.
    /// </summary>
    /// <inheritdoc cref="AddParameter"/>
    public TestDirectivePatternBuilder AddNumberParameter(string? parameterName = null)
    {
        _baseBuilder.AddNumberParameter(parameterName);
        return this;
    }

    /// <summary>
    /// Adds a parameter for HTTP status codes.
    /// </summary>
    /// <inheritdoc cref="AddParameter"/>
    public TestDirectivePatternBuilder AddStatusCodesParameter(string? parameterName = null)
    {
        _baseBuilder.AddStatusCodesParameter(parameterName);
        return this;
    }

    /// <summary>
    /// Adds a parameter for HTTP header names.
    /// </summary>
    /// <inheritdoc cref="AddParameter"/>
    public TestDirectivePatternBuilder AddHeaderNameParameter(string? parameterName = null)
    {
        _baseBuilder.AddHeaderNameParameter(parameterName);
        return this;
    }

    /// <summary>
    /// Adds a <see cref="DateTime"/> parameter.
    /// </summary>
    /// <inheritdoc cref="AddParameter"/>
    public TestDirectivePatternBuilder AddDateTimeParameter(string? parameterName = null)
    {
        _baseBuilder.AddDateTimeParameter(parameterName);
        return this;
    }

    /// <summary>
    /// Adds a <see cref="TimeOnly"/> parameter.
    /// </summary>
    /// <inheritdoc cref="AddParameter"/>
    public TestDirectivePatternBuilder AddTimeOnlyParameter(string? parameterName = null)
    {
        _baseBuilder.AddTimeOnlyParameter(parameterName);
        return this;
    }

    /// <summary>
    /// Adds a parameter that accepts an array of <see cref="string"/>.
    /// </summary>
    /// <inheritdoc cref="AddParameter"/>
    public TestDirectivePatternBuilder AddStringArrayParameter(string? parameterName = null)
    {
        _baseBuilder.AddStringArrayParameter(parameterName);
        return this;
    }

    /// <summary>
    /// Adds a parameter that accepts an array of <see cref="bool"/>.
    /// </summary>
    /// <inheritdoc cref="AddParameter"/>
    public TestDirectivePatternBuilder AddBooleanArrayParameter(string? parameterName = null)
    {
        _baseBuilder.AddBooleanArrayParameter(parameterName);
        return this;
    }

    /// <summary>
    /// Adds a parameter that accepts an array of numbers.
    /// </summary>
    /// <inheritdoc cref="AddParameter"/>
    public TestDirectivePatternBuilder AddNumberArrayParameter(string? parameterName = null)
    {
        _baseBuilder.AddNumberArrayParameter(parameterName);
        return this;
    }
}
