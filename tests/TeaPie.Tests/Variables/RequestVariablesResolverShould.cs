using FluentAssertions;
using TeaPie.Http;
using TeaPie.TestCases;
using TeaPie.Variables;

namespace TeaPie.Tests.Variables;

public class RequestVariablesResolverShould
{
    [Fact]
    public async Task ResolveRequestWholeBodyVariable()
    {
        const string requestName = "MyRequest";
        const string bodyContent = "Hello World!";
        var variable = string.Join(HttpFileParserConstants.RequestVariableSeparator,
            requestName,
            HttpFileParserConstants.RequestSelector,
            HttpFileParserConstants.BodySelector,
            HttpFileParserConstants.WholeBodySelector);

        var testCaseContext = new TestCaseExecutionContext(null!);
        var requestContext = new RequestExecutionContext(null!, testCaseContext);

        var requestMessage = new HttpRequestMessage
        {
            Content = new StringContent(bodyContent)
        };

        testCaseContext.Requests.Add(requestName, requestMessage);

        if (RequestVariablesResolver.TryGetVariableDescription(variable, out var description))
        {
            var resolver = new RequestVariablesResolver(description);
            var resolved = await resolver.Resolve(requestContext);

            resolved.Should().BeEquivalentTo(bodyContent);
        }
    }

    [Fact]
    public async Task ResolveResponseWholeBodyVariable()
    {
        const string requestName = "MyRequest";
        const string bodyContent = "Hello World!";
        var variable = string.Join(HttpFileParserConstants.RequestVariableSeparator,
            requestName,
            HttpFileParserConstants.ResponseSelector,
            HttpFileParserConstants.BodySelector,
            HttpFileParserConstants.WholeBodySelector);

        var testCaseContext = new TestCaseExecutionContext(null!);
        var requestContext = new RequestExecutionContext(null!, testCaseContext);

        var responseMessage = new HttpResponseMessage
        {
            Content = new StringContent(bodyContent)
        };

        testCaseContext.Responses.Add(requestName, responseMessage);

        if (RequestVariablesResolver.TryGetVariableDescription(variable, out var description))
        {
            var resolver = new RequestVariablesResolver(description);
            var resolved = await resolver.Resolve(requestContext);

            resolved.Should().BeEquivalentTo(bodyContent);
        }
    }
}
