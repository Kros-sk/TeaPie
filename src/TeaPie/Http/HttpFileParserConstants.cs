namespace TeaPie.Http;

internal static class HttpFileParserConstants
{
    private const string SimpleNamePattern = "[a-zA-Z0-9_-]+";
    private const string StructureVariableNamePatternBase = "[a-zA-Z0-9_.-]+";
    public const string VariableNamePattern = "^" + StructureVariableNamePatternBase + "$";
    public const string VariableNotationPattern = "{{(" + StructureVariableNamePatternBase + ")}}";

    public const string RequestNameMetadataGroupName = "name";
    public const string RequestNameMetadataPattern =
        @"@name\s*=\s*(?<" + RequestNameMetadataGroupName + ">" + StructureVariableNamePatternBase + ")";

    public const string RequestVariableSeparator = ".";
    public const string RequestSelector = "request";
    public const string ResponseSelector = "response";
    public const string BodySelector = "body";
    public const string HeadersSelector = "headers";
    public const string WholeBodySelector = "*";

    public const string HeaderNamePattern = SimpleNamePattern;
    public const string RequestVariablePattern =
        "^" + SimpleNamePattern + @"\" + RequestVariableSeparator +
        "(" + RequestSelector + "|" + ResponseSelector + @")\" + RequestVariableSeparator +
        "(" + BodySelector + "|" + HeadersSelector + @")\" + RequestVariableSeparator +
        @"(\*|\$[^\s]+|\/[^\/]+\/[^\/]+|" + HeaderNamePattern + ")$";

    public const string HttpHeaderSeparator = ":";
    public const string HttpCommentPrefix = "# ";
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

    public static readonly Dictionary<string, HttpMethod> HttpMethodsMap =
        new(StringComparer.OrdinalIgnoreCase)
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
}
