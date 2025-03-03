namespace TeaPie.Http.Parsing;

internal static class HttpFileParserConstants
{
    #region Naming Patterns

    private const string SimpleNamePattern = "[a-zA-Z0-9_-]+";
    private const string StructureVariableNamePatternBase = "[a-zA-Z0-9_.$-]+";
    public const string VariableNamePattern = "^" + StructureVariableNamePatternBase + "$";
    public const string VariableNotationPattern = "{{(" + StructureVariableNamePatternBase + ")}}";

    public const string HeaderNameBasePattern = "[A-Za-z0-9!#$%&'*+.^_`|~-]+";
    public const string HeaderNamePattern = "^" + HeaderNameBasePattern + "$";
    public const string HeaderValuePattern = @"^[\t\x20-\x7E\x80-\xFF]*$";

    public const string RequestNameMetadataGroupName = "name";
    public const string RequestNameMetadataPattern =
        @"@name\s+(?<" + RequestNameMetadataGroupName + ">" + SimpleNamePattern + ")";

    #endregion

    #region Directives

    public const string DirectivePrefixPattern = @"^##\s*";

    #region Authentication Directives

    public const string AuthDirectivePrefix = "AUTH-";

    public const string AuthProviderDirectiveName = "PROVIDER";
    public const string AuthProviderDirectiveFullName = AuthDirectivePrefix + AuthProviderDirectiveName;
    public const string AuthProviderDirectiveParameterName = "AuthProvider";
    public static readonly string AuthProviderSelectorDirectivePattern =
        HttpDirectivePatternBuilder.Create(AuthProviderDirectiveName)
            .WithPrefix(AuthDirectivePrefix)
            .AddStringParameter(AuthProviderDirectiveParameterName)
            .Build();

    #endregion

    #region Retry Directives

    public const string RetryDirectivePrefix = "RETRY-";

    public const string RetryStrategyDirectiveName = "STRATEGY";
    public const string RetryStrategyDirectiveFullName = RetryDirectivePrefix + RetryStrategyDirectiveName;
    public const string RetryStrategyDirectiveParameterName = "StrategyName";
    public static readonly string RetryStrategySelectorDirectivePattern =
        HttpDirectivePatternBuilder.Create(RetryStrategyDirectiveName)
            .WithPrefix(RetryDirectivePrefix)
            .AddStringParameter(RetryStrategyDirectiveParameterName)
            .Build();

    public const string RetryUntilStatusCodesDirectiveName = "UNTIL-STATUS";
    public const string RetryUntilStatusCodesDirectiveFullName = RetryDirectivePrefix + RetryUntilStatusCodesDirectiveName;
    public const string RetryUntilStatusCodesDirectiveParameterName = "StatusCodes";
    public static readonly string RetryUntilStatusCodesDirectivePattern =
        HttpDirectivePatternBuilder.Create(RetryUntilStatusCodesDirectiveName)
            .WithPrefix(RetryDirectivePrefix)
            .AddStatusCodesParameter(RetryUntilStatusCodesDirectiveParameterName)
            .Build();

    public const string RetryMaxAttemptsDirectiveName = "MAX-ATTEMPTS";
    public const string RetryMaxAttemptsDirectiveFullName = RetryDirectivePrefix + RetryMaxAttemptsDirectiveName;
    public const string RetryMaxAttemptsDirectiveParameterName = "MaxAttempts";
    public static readonly string RetryMaxAttemptsDirectivePattern =
        HttpDirectivePatternBuilder.Create(RetryMaxAttemptsDirectiveName)
            .WithPrefix(RetryDirectivePrefix)
            .AddNumberParameter(RetryMaxAttemptsDirectiveParameterName)
            .Build();

    public const string RetryBackoffTypeDirectiveName = "BACKOFF-TYPE";
    public const string RetryBackoffTypeDirectiveFullName = RetryDirectivePrefix + RetryBackoffTypeDirectiveName;
    public const string RetryBackoffTypeDirectiveParameterName = "BackoffType";
    public static readonly string RetryBackoffTypeDirectivePattern =
        HttpDirectivePatternBuilder.Create(RetryBackoffTypeDirectiveName)
            .WithPrefix(RetryDirectivePrefix)
            .AddStringParameter(RetryBackoffTypeDirectiveParameterName)
            .Build();

    public const string RetryMaxDelayDirectiveName = "MAX-DELAY";
    public const string RetryMaxDelayDirectiveFullName = RetryDirectivePrefix + RetryMaxDelayDirectiveName;
    public const string RetryMaxDelayDirectiveParameterName = "MaxDelay";
    public static readonly string RetryMaxDelayDirectivePattern =
        HttpDirectivePatternBuilder.Create(RetryMaxDelayDirectiveName)
            .WithPrefix(RetryDirectivePrefix)
            .AddTimeOnlyParameter(RetryMaxDelayDirectiveParameterName)
            .Build();

    #endregion

    #region Test Directives

    public const string TestDirectivePrefix = "TEST-";
    public const string TestDirectiveParameterName = "DirectiveName";

    public const string TestDirectivePattern = @"^##\s*TEST-[A-Za-z0-9_-]+(?:\s*:\s*.+)?\s*$";

    public const string TestExpectStatusCodesDirectiveName = "EXPECT-STATUS";
    public const string TestExpectStatusCodesDirectiveFullName = TestDirectivePrefix + TestExpectStatusCodesDirectiveName;
    public const string TestExpectStatusCodesParameterName = "StatusCodes";
    public static readonly string TestExpectStatusCodesDirectivePattern =
        HttpDirectivePatternBuilder.Create(TestExpectStatusCodesDirectiveName)
            .WithPrefix(TestDirectivePrefix)
            .AddStatusCodesParameter(TestExpectStatusCodesParameterName)
            .Build();

    public const string TestHasBodyDirectiveName = "HAS-BODY";
    public const string TestHasBodyDirectiveFullName = TestDirectivePrefix + TestHasBodyDirectiveName;
    public const string TestHasBodyDirectiveParameterName = "Bool";
    public static readonly string TestHasBodyDirectivePattern =
        HttpDirectivePatternBuilder.Create(TestHasBodyDirectiveName)
            .WithPrefix(TestDirectivePrefix)
            .AddBooleanParameter(TestHasBodyDirectiveParameterName)
            .Build();

    public const string TestHasBodyNoParameterInternalDirectiveName = "HAS-BODY-SIMPLIFIED";
    public const string TestHasBodyNoParameterInternalDirectiveFullName =
        TestDirectivePrefix + TestHasBodyNoParameterInternalDirectiveName;
    public static readonly string TestHasBodyNoParameterDirectivePattern =
        HttpDirectivePatternBuilder.Create(TestHasBodyDirectiveName)
            .WithPrefix(TestDirectivePrefix)
            .Build();

    public const string TestHasHeaderDirectiveName = "HAS-HEADER";
    public const string TestHasHeaderDirectiveFullName = TestDirectivePrefix + TestHasHeaderDirectiveName;
    public const string TestHasHeaderDirectiveParameterName = "HeaderName";
    public static readonly string TestHasHeaderDirectivePattern =
        HttpDirectivePatternBuilder.Create(TestHasHeaderDirectiveName)
            .WithPrefix(TestDirectivePrefix)
            .AddHeaderNameParameter(TestHasHeaderDirectiveParameterName)
            .Build();

    #endregion

    #endregion

    #region Request

    #region Request Variables

    public const string RequestVariableSeparator = ".";
    public const string RequestSelector = "request";
    public const string ResponseSelector = "response";
    public const string BodySelector = "body";
    public const string HeadersSelector = "headers";
    public const string WholeBodySelector = "*";

    public const string RequestVariablePattern =
        "^" + SimpleNamePattern + @"\" + RequestVariableSeparator +
        "(" + RequestSelector + "|" + ResponseSelector + @")\" + RequestVariableSeparator +
        "(" + BodySelector + "|" + HeadersSelector + @")\" + RequestVariableSeparator +
        @"(\*|(\$[^\s]+)|([A-Za-z0-9!#$%&'*+.^_`|~-]+(\.[A-Za-z0-9!#$%&'*+.^_`|~-]+)*)|)";

    #endregion

    #region Request Definition

    public const string RequestMethodAndUriLinePattern =
        @"\b(GET|POST|PUT|DELETE|HEAD|OPTIONS|PATCH|TRACE)\b\s+.+";

    public const string HttpHeaderSeparator = ":";
    public const string HttpCommentPrefix = "# ";
    public const string HttpCommentAltPrefix = "// ";
    public const string HttpDirectivePrefix = "## ";
    public const string HttpRequestSeparatorDirectiveLineRegex = "###.*";

    public const string HttpGetMethodDirective = "GET";
    public const string HttpPutMethodDirective = "PUT";
    public const string HttpPostMethodDirective = "POST";
    public const string HttpPatchMethodDirective = "PATCH";
    public const string HttpDeleteMethodDirective = "DELETE";
    public const string HttpHeadMethodDirective = "HEAD";
    public const string HttpOptionsMethodDirective = "OPTIONS";
    public const string HttpTraceMethodDirective = "TRACE";

    public static readonly IReadOnlyDictionary<string, HttpMethod> HttpMethodsMap =
        new Dictionary<string, HttpMethod>(StringComparer.OrdinalIgnoreCase)
        {
            { HttpGetMethodDirective, HttpMethod.Get },
            { HttpPutMethodDirective, HttpMethod.Put },
            { HttpPostMethodDirective, HttpMethod.Post },
            { HttpPatchMethodDirective, HttpMethod.Patch },
            { HttpDeleteMethodDirective, HttpMethod.Delete },
            { HttpHeadMethodDirective, HttpMethod.Head },
            { HttpOptionsMethodDirective, HttpMethod.Options },
            { HttpTraceMethodDirective, HttpMethod.Trace }
        };

    #endregion

    #region Headers

    public static readonly List<string> SpecialHeaders =
        [
            "Content-Type",
            "Content-Disposition",
            "Content-Encoding",
            "Content-Language",
            "Expect",
            "Authorization",
            "User-Agent",
            "Date",
            "Connection"
        ];

    #endregion

    #endregion
}
