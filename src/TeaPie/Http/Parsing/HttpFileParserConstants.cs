namespace TeaPie.Http.Parsing;

internal static class HttpFileParserConstants
{
    private const string SimpleNamePattern = "[a-zA-Z0-9_-]+";
    private const string StructureVariableNamePatternBase = "[a-zA-Z0-9_.$-]+";
    public const string VariableNamePattern = "^" + StructureVariableNamePatternBase + "$";
    public const string VariableNotationPattern = "{{(" + StructureVariableNamePatternBase + ")}}";

    public const string HeaderNamePattern = "^[A-Za-z0-9!#$%&'*+.^_`|~-]+$";
    public const string HeaderValuePattern = @"^[\t\x20-\x7E\x80-\xFF]*$";

    public const string RequestNameMetadataGroupName = "name";
    public const string RequestNameMetadataPattern =
        @"@name\s+(?<" + RequestNameMetadataGroupName + ">" + SimpleNamePattern + ")";

    public const string RequestVariableSeparator = ".";
    public const string RequestSelector = "request";
    public const string ResponseSelector = "response";
    public const string BodySelector = "body";
    public const string HeadersSelector = "headers";
    public const string WholeBodySelector = "*";

    public const string RetryStrategyDirectiveName = "RETRY-STRATEGY";
    public const string RetryStrategySelectorDirectivePattern =
        @"^##\s*" + RetryStrategyDirectiveName + @":\s*(?<StrategyName>.+?)\s*$";

    public const string RetryUntilStatusCodesDirectiveName = "RETRY-UNTIL-STATUS";
    public const string RetryUntilStatusCodesDirectivePattern =
        @"^##\s*" + RetryUntilStatusCodesDirectiveName + @":\s*\[(?<StatusCodes>[0-9,\s]+)\]\s*$";

    public const string RetryMaxAttemptsDirectiveName = "RETRY-MAX-ATTEMPTS";
    public const string RetryMaxAttemptsDirectivePattern =
        @"^##\s*" + RetryMaxAttemptsDirectiveName + @":\s*(?<MaxAttempts>\d+)\s*$";

    public const string RetryBackoffTypeDirectiveName = "RETRY-BACKOFF-TYPE";
    public const string RetryBackoffTypeDirectivePattern =
        @"^##\s*" + RetryBackoffTypeDirectiveName + @":\s*(?<BackoffType>\w+)\s*$";

    public const string RetryMaxDelayDirectiveName = "RETRY-MAX-DELAY";
    public const string RetryMaxDelayDirectivePattern =
        @"^##\s*" + RetryMaxDelayDirectiveName + @":\s*(?<MaxDelay>\d{2}:\d{2}:\d{2}(?:\.\d{1,3})?)\s*$";

    public const string RequestVariablePattern =
        "^" + SimpleNamePattern + @"\" + RequestVariableSeparator +
        "(" + RequestSelector + "|" + ResponseSelector + @")\" + RequestVariableSeparator +
        "(" + BodySelector + "|" + HeadersSelector + @")\" + RequestVariableSeparator +
        @"(\*|(\$[^\s]+)|([A-Za-z0-9!#$%&'*+.^_`|~-]+(\.[A-Za-z0-9!#$%&'*+.^_`|~-]+)*)|)";

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
}
