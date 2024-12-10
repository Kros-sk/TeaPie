using FluentAssertions;
using System.Net.Http.Headers;
using TeaPie.Http;
using TeaPie.TestCases;
using TeaPie.Variables;
using Consts = TeaPie.Http.HttpFileParserConstants;

namespace TeaPie.Tests.Variables;

public class RequestVariablesResolverShould
{
    [Theory]
    [MemberData(nameof(GetRequestVariablesTestCases))]
    public void DetermineWhetherItIsRequestVariable(string variable, bool isValid)
    {
        RequestVariablesResolver.IsRequestVariable(variable).Should().Be(isValid);
    }

    [Theory]
    [MemberData(nameof(GetVariableDescriptionTestCases))]
    public void ReturnProperRequestVariableDescription(
        string variableName,
        bool expectedResult,
        string? expectedRequestName,
        string? expectedType,
        string? expectedContent,
        string? expectedQuery)
    {
        var result = RequestVariablesResolver.TryGetVariableDescription(variableName, out var description);

        result.Should().Be(expectedResult);

        if (expectedResult)
        {
            description.Should().NotBeNull();
            description!.Name.Should().Be(expectedRequestName);
            description.Type.Should().Be(expectedType);
            description.Content.Should().Be(expectedContent);
            description.Query.Should().Be(expectedQuery);
        }
        else
        {
            description.Should().BeNull();
        }
    }

    [Fact]
    public async Task ResolveRequestVariableWithWholeBodyCorrectly()
        => await AssertResolvedVariableAsync(
            requestName: "MyRequest",
            contentType: "text/plain",
            bodyContent: "Hello World!",
            type: Consts.RequestSelector,
            bodyOrHeaders: Consts.BodySelector,
            query: Consts.WholeBodySelector,
            headers: [],
            expectedValue: "Hello World!");

    [Fact]
    public async Task ResolveResponseVariableWithWholeBodyCorrectly()
        => await AssertResolvedVariableAsync(
            requestName: "MyRequest",
            contentType: "text/plain",
            bodyContent: "Hello World!",
            type: Consts.ResponseSelector,
            bodyOrHeaders: Consts.BodySelector,
            query: Consts.WholeBodySelector,
            headers: [],
            expectedValue: "Hello World!",
            isResponse: true);

    [Theory]
    [MemberData(nameof(GetQueryTestCases))]
    public async Task ResolveRequestVariableWithBodyQueriesCorrectly(
        string requestName,
        string contentType,
        string bodyContent,
        string path,
        string expectedValue)
        => await AssertResolvedVariableAsync(
             requestName: requestName,
             contentType: contentType,
             bodyContent: bodyContent,
             type: Consts.RequestSelector,
             bodyOrHeaders: Consts.BodySelector,
             query: path,
             headers: [],
             expectedValue: expectedValue);

    [Theory]
    [MemberData(nameof(GetQueryTestCases))]
    public async Task ResolveResponseVariableWithBodyQueriesCorrectly(
        string requestName,
        string contentType,
        string bodyContent,
        string path,
        string expectedValue)
        => await AssertResolvedVariableAsync(
            requestName: requestName,
            contentType: contentType,
            bodyContent: bodyContent,
            type: Consts.ResponseSelector,
            bodyOrHeaders: Consts.BodySelector,
            query: path,
            headers: [],
            expectedValue: expectedValue,
            isResponse: true);

    [Fact]
    public async Task ResolveRequestVariableWithHeaderCorrectly()
        => await AssertResolvedVariableAsync(
            requestName: "MyRequest",
            contentType: "text/plain",
            bodyContent: string.Empty,
            type: Consts.RequestSelector,
            bodyOrHeaders: Consts.HeadersSelector,
            query: "Authorization",
            headers: [new("Authorization", "authToken")],
            expectedValue: "authToken");

    [Fact]
    public async Task ResolveResponseVariableWithHeaderCorrectly()
        => await AssertResolvedVariableAsync(
            requestName: "MyRequest",
            contentType: "text/plain",
            bodyContent: string.Empty,
            type: Consts.ResponseSelector,
            bodyOrHeaders: Consts.HeadersSelector,
            query: "Authorization",
            headers: [new("Authorization", "authToken")],
            expectedValue: "authToken",
            isResponse: true);

    private static async Task AssertResolvedVariableAsync(
        string requestName,
        string contentType,
        string bodyContent,
        string type,
        string bodyOrHeaders,
        string query,
        KeyValuePair<string, string>[] headers,
        string expectedValue,
        bool isResponse = false)
    {
        var variableDescription = new RequestVariableDescription(requestName, type, bodyOrHeaders, query);
        var requestContext = isResponse
            ? GetRequestContextWithResponse(requestName, contentType, bodyContent, headers)
            : GetRequestContext(requestName, contentType, bodyContent, headers);

        var resolver = new RequestVariablesResolver(variableDescription);
        var resolved = await resolver.Resolve(requestContext);

        resolved.Should().BeEquivalentTo(expectedValue);
    }

    private static RequestExecutionContext GetRequestContext(
        string requestName,
        string contentType,
        string bodyContent,
        KeyValuePair<string, string>[] headers)
    {
        (var testCaseContext, var requestContext) = PrepareContexts(requestName);
        var content = PrepareContent(contentType, bodyContent);

        var requestMessage = new HttpRequestMessage()
        {
            Content = content
        };

        AddHeaders(requestMessage.Headers, headers);

        requestContext.Request = requestMessage;
        testCaseContext.Request = requestMessage;
        testCaseContext.Requests.Add(requestName, requestMessage);

        return requestContext;
    }

    private static StringContent PrepareContent(string contentType, string bodyContent)
    {
        var content = new StringContent(bodyContent);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return content;
    }

    private static (TestCaseExecutionContext, RequestExecutionContext) PrepareContexts(string requestName)
    {
        var testCaseContext = new TestCaseExecutionContext(null!);
        var requestContext = new RequestExecutionContext(null!, testCaseContext)
        {
            Name = requestName
        };

        return (testCaseContext, requestContext);
    }

    private static RequestExecutionContext GetRequestContextWithResponse(
        string requestName,
        string contentType,
        string bodyContent,
        KeyValuePair<string, string>[] headers)
    {
        (var testCaseContext, var requestContext) = PrepareContexts(requestName);
        var content = PrepareContent(contentType, bodyContent);

        var responseMessage = new HttpResponseMessage
        {
            Content = content
        };

        AddHeaders(responseMessage.Headers, headers);

        requestContext.Response = responseMessage;
        testCaseContext.Response = responseMessage;
        testCaseContext.Responses.Add(requestName, responseMessage);

        return requestContext;
    }

    private static void AddHeaders(HttpHeaders headersCollection, KeyValuePair<string, string>[] headersToAdd)
    {
        foreach (var header in headersToAdd)
        {
            headersCollection.Add(header.Key, header.Value);
        }
    }

    public static IEnumerable<object[]> GetRequestVariablesTestCases()
    {
        const string separator = Consts.RequestVariableSeparator;

        yield return new object[] { "randomVariable", false };
        yield return new object[] { "random.Variable", false };
        yield return new object[] { "random.Pseudo.Request.Variable.It.Is", false };
        yield return new object[] { string.Join(separator, "requestName", Consts.RequestSelector), false };
        yield return new object[] { string.Join(separator, "requestName", Consts.ResponseSelector), false };
        yield return new object[] { string.Join(separator, "requestName", Consts.RequestSelector, Consts.BodySelector), false };
        yield return new object[] { string.Join(separator, "requestName", Consts.RequestSelector,
            Consts.HeadersSelector), false };
        yield return new object[] { string.Join(separator, "requestName", Consts.ResponseSelector,
            Consts.BodySelector), false };
        yield return new object[] { string.Join(separator, "requestName", Consts.ResponseSelector,
            Consts.HeadersSelector), false };

        yield return new object[] { string.Join(separator, "requestName", Consts.RequestSelector,
            Consts.BodySelector, Consts.WholeBodySelector), true };
        yield return new object[] { string.Join(separator, "requestName", Consts.RequestSelector,
            Consts.BodySelector, "$", "id"), true };
        yield return new object[] { string.Join(separator, "requestName", Consts.RequestSelector,
            Consts.BodySelector, "$", "items[0]", "id"), true };
        yield return new object[] { string.Join(separator, "requestName", Consts.RequestSelector,
            Consts.BodySelector, "id"), true };
        yield return new object[] { string.Join(separator, "requestName", Consts.RequestSelector,
            Consts.BodySelector, "/items[0]/id"), true };

        yield return new object[] { string.Join(separator, "requestName", Consts.RequestSelector,
            Consts.HeadersSelector, "Authorization"), true };
        yield return new object[] { string.Join(separator, "requestName", Consts.RequestSelector,
            Consts.HeadersSelector, "Last-Modified"), true };

        yield return new object[] { string.Join(separator, "requestName", Consts.ResponseSelector,
            Consts.BodySelector, Consts.WholeBodySelector), true };
        yield return new object[] { string.Join(separator, "requestName", Consts.ResponseSelector,
            Consts.BodySelector, "$", "id"), true };
        yield return new object[] { string.Join(separator, "requestName", Consts.ResponseSelector,
            Consts.BodySelector, "$", "items[0]", "id"), true };
        yield return new object[] { string.Join(separator, "requestName", Consts.ResponseSelector,
            Consts.BodySelector, "id"), true };
        yield return new object[] { string.Join(separator, "requestName", Consts.ResponseSelector,
            Consts.BodySelector, "/items[0]/id"), true };

        yield return new object[] { string.Join(separator, "requestName", Consts.ResponseSelector,
            Consts.HeadersSelector, "Authorization"), true };
        yield return new object[] { string.Join(separator, "requestName", Consts.ResponseSelector,
            Consts.HeadersSelector, "Last-Modified"), true };
    }

    public static IEnumerable<object?[]> GetVariableDescriptionTestCases()
    {
        yield return new object?[] { "InvalidFormat", false, null, null, null, null };
        yield return new object?[] { "", false, null, null, null, null };
        yield return new object?[] { "requestName.request.body", false, null, null, null, null };
        yield return new object[] { "MyRequest.request.body.$.id", true, "MyRequest", "request", "body", "$.id" };
        yield return new object[] { "MyRequest.response.headers.Authorization", true, "MyRequest", "response", "headers", "Authorization" };
        yield return new object[] { "MyRequest.response.body.*", true, "MyRequest", "response", "body", "*" };
    }

    public static IEnumerable<object[]> GetQueryTestCases()
    {
        yield return new object[]
        {
            "MyRequest",
            "application/json",
            "{\"id\": \"12345\", \"name\": \"John Doe\"}",
            "$.id",
            "12345"
        };
        yield return new object[]
        {
            "MyRequest",
            "application/json",
            "{\"user\": {\"id\": \"12345\", \"name\": \"John Doe\"}}",
            "$.user.id",
            "12345"
        };
        yield return new object[]
        {
            "MyRequest",
            "application/xml",
            "<response><id>12345</id></response>",
            "/response/id",
            "12345"
        };
        yield return new object[]
        {
            "MyRequest",
            "application/xml",
            "<response><user><id>12345</id></user></response>",
            "/response/user/id",
            "12345"
        };
    }
}
