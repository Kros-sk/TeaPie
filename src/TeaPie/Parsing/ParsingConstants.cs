namespace TeaPie.Parsing;

internal static class ParsingConstants
{
    public const string LoadScriptDirective = "#load";
    public const string NuGetDirective = "#nuget";

    public const string LoadDirectivePattern = @"^#load\s+""([a-zA-Z0-9_\-\.\s\\\/]+\.([a-zA-Z0-9]+))""$";
    public const string NuGetDirectivePattern = @"^#nuget\s+""([a-zA-Z0-9_.-]+),\s*([0-9]+\.[0-9]+\.[0-9]+)""$";

    public const string HttpHeaderSeparator = ":";
    public const string HttpCommentPrefix = "# ";
    public const string HttpDirectivePrefix = "## ";
    public const string HttpRequestSeparatorDirective = "### ";

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
