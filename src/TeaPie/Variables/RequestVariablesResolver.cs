using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using TeaPie.Http;

namespace TeaPie.Variables;

internal partial class RequestVariablesResolver(RequestVariableDescription requestVariable)
{
    private readonly IBodyResolver[] _bodyResolvers =
        [
            new JsonBodyResolver(),
            new XmlBodyResolver()
        ];

    private readonly RequestVariableDescription _requestVariable = requestVariable;

    public async Task<string> Resolve(RequestExecutionContext executionContext)
    {
        if (executionContext.TestCaseExecutionContext is null)
        {
            throw new InvalidOperationException("Unable to resolve request variable if test execution context is null.");
        }

        if (IsRequest() && TryGetHttpRequestMessage(executionContext, out var request))
        {
            return await Resolve(request.Headers, request.Content);
        }
        else if (IsResponse() && TryGetHttpResponseMessage(executionContext, out var response))
        {
            return await Resolve(response.Headers, response.Content);
        }

        return _requestVariable.ToString();
    }

    private async Task<string> Resolve(HttpHeaders headers, HttpContent? content)
    {
        if (IsHeaders())
        {
            return ResolveHeaders(headers);
        }
        else if (IsBody())
        {
            if (content is not null)
            {
                var body = await content.ReadAsStringAsync();
                var contentType = content.Headers.ContentType?.MediaType;
                return ResolveBody(body, contentType);
            }

            return string.Empty;
        }

        return _requestVariable.ToString();
    }

    private string ResolveHeaders(HttpHeaders headers)
        => headers.TryGetValues(_requestVariable.Query, out var values) ? values.Last() : _requestVariable.ToString();

    private string ResolveBody(string body, string? contentType)
    {
        if (_requestVariable.Query.Equals("*") || contentType is null)
        {
            return body;
        }

        foreach (var resolver in _bodyResolvers)
        {
            if (resolver.CanResolve(contentType))
            {
                return resolver.Resolve(body, _requestVariable.Query, _requestVariable.ToString());
            }
        }

        return body;
    }

    private bool TryGetHttpRequestMessage(
        RequestExecutionContext executionContext,
        [NotNullWhen(true)] out HttpRequestMessage? request)
        => executionContext.TestCaseExecutionContext!.Requests.TryGetValue(_requestVariable.Name, out request);

    private bool TryGetHttpResponseMessage(
        RequestExecutionContext executionContext,
        [NotNullWhen(true)] out HttpResponseMessage? response)
        => executionContext.TestCaseExecutionContext!.Responses.TryGetValue(_requestVariable.Name, out response);

    private bool IsRequest()
        => _requestVariable.Type.Equals(HttpFileParserConstants.RequestSelector, StringComparison.OrdinalIgnoreCase);

    private bool IsResponse()
        => _requestVariable.Type.Equals(HttpFileParserConstants.ResponseSelector, StringComparison.OrdinalIgnoreCase);

    private bool IsBody()
        => _requestVariable.Content.Equals(HttpFileParserConstants.BodySelector, StringComparison.OrdinalIgnoreCase);

    private bool IsHeaders()
        => _requestVariable.Content.Equals(HttpFileParserConstants.HeadersSelector, StringComparison.OrdinalIgnoreCase);

    public static bool IsRequestVariable(string variableName)
        => RequestVariableNamePattern().Match(variableName).Success;

    public static bool TryGetVariableDescription(string variableName, [NotNullWhen(true)] out RequestVariableDescription? description)
    {
        var segments = variableName.Split('.');
        if (segments.Length < 4)
        {
            description = null;
            return false;
        }

        description = new RequestVariableDescription(
            segments[0],
            segments[1],
            segments[2],
            string.Join(HttpFileParserConstants.RequestVariableSeparator, segments.Skip(3)));

        return true;
    }

    [GeneratedRegex(HttpFileParserConstants.RequestVariablePattern)]
    private static partial Regex RequestVariableNamePattern();
}
