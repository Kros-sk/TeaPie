namespace TeaPie.Http.Parsing;

internal static class HttpFileParserConstants
{
    #region Naming Patterns

    private const string SimpleNamePattern = "[a-zA-Z0-9_-]+";
    private const string StructureVariableNamePatternBase = "(?!\\$)[a-zA-Z0-9_.-][a-zA-Z0-9_.$-]*";
    public const string VariableNamePattern = "^" + StructureVariableNamePatternBase + "$";
    public const string VariableNotationPattern = "{{(" + StructureVariableNamePatternBase + ")}}";

    private const string StructureFunctionNamePatternBase = "\\$[a-zA-Z0-9_.$-]*";
    public const string FunctionNamePattern = "^" + StructureFunctionNamePatternBase + "$";
    public const string FunctionNotationPattern = "{{(\\$[a-zA-Z0-9_]+(?:\\s+[^}]*)?)}}";

    public const string HeaderNameBasePattern = "[A-Za-z0-9!#$%&'*+.^_`|~-]+";
    public const string HeaderNamePattern = "^" + HeaderNameBasePattern + "$";
    public const string HeaderValuePattern = @"^[\t\x20-\x7E\x80-\xFF]*$";

    public const string RequestNameMetadataGroupName = "name";
    public const string RequestNameMetadataPattern =
        @"@name\s+(?<" + RequestNameMetadataGroupName + ">" + SimpleNamePattern + ")";

    #endregion

    #region Directives

    public const string DirectivePrefixPattern = @"^##\s*";

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
    public const string HttpRequestSeparatorDirectiveLineRegex = "###.*(\r?\n|$)";

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
