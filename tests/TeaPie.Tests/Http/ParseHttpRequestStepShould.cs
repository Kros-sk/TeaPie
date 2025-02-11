using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using TeaPie.Http;
using TeaPie.Http.Headers;
using TeaPie.Http.Retrying;
using TeaPie.TestCases;
using TeaPie.Variables;

namespace TeaPie.Tests.Http;

public class ParseHttpRequestStepShould
{
    [Fact]
    public async Task ThrowProperExceptionWhenRequestContextIsWithoutRawContent()
    {
        var context = RequestHelper.PrepareRequestContext(RequestsIndex.RequestWithFullStructure, false);

        var appContext = new ApplicationContextBuilder()
            .WithPath(RequestsIndex.RootFolderFullPath)
            .Build();

        var accessor = new RequestExecutionContextAccessor() { Context = context };

        var parser = CreateParser();
        var step = new ParseHttpRequestStep(accessor, parser);

        await step.Invoking(async step => await step.Execute(appContext)).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AssignRequestMessageCorrectly()
    {
        var context = RequestHelper.PrepareRequestContext(RequestsIndex.RequestWithFullStructure);

        var appContext = new ApplicationContextBuilder()
            .WithPath(RequestsIndex.RootFolderFullPath)
            .Build();

        var accessor = new RequestExecutionContextAccessor() { Context = context };

        var parser = CreateParser();
        var step = new ParseHttpRequestStep(accessor, parser);

        await step.Execute(appContext);

        context.Request.Should().NotBeNull();
        context.Request!.Method.Should().Be(HttpMethod.Post);
        context.Request!.Content.Should().NotBeNull();
        context.Request!.Headers.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CallParseMethodOnParserDuringExecution()
    {
        var context = RequestHelper.PrepareRequestContext(RequestsIndex.PlainGetRequestPath);

        var appContext = new ApplicationContextBuilder()
            .WithPath(RequestsIndex.RootFolderFullPath)
            .Build();

        var accessor = new RequestExecutionContextAccessor() { Context = context };

        var parser = Substitute.For<IHttpRequestParser>();
        var step = new ParseHttpRequestStep(accessor, parser);

        await step.Execute(appContext);

        parser.Received(1).Parse(context);
    }

    [Fact]
    public async Task AddNamedRequestToParentTestCaseRequests()
    {
        const string RequestName = "NamedRequest";
        var parser = CreateParser();

        var testCaseContext = new TestCaseExecutionContext(null!);
        var context = RequestHelper.PrepareRequestContext(RequestsIndex.RequestWithNamePath);
        context.TestCaseExecutionContext = testCaseContext;

        var appContext = new ApplicationContextBuilder()
            .WithPath(RequestsIndex.RootFolderFullPath)
            .Build();

        var accessor = new RequestExecutionContextAccessor() { Context = context };

        var step = new ParseHttpRequestStep(accessor, parser);

        await step.Execute(appContext);

        context.TestCaseExecutionContext.Request.Should().Be(context.Request);
        context.TestCaseExecutionContext.Requests.ContainsKey(RequestName).Should().BeTrue();
    }

    private static HttpRequestParser CreateParser()
    {
        var services = new ServiceCollection();
        services.AddHttpClient();

        var serviceProvider = services.BuildServiceProvider();

        var clientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var headersProvider = new HttpRequestHeadersProvider(clientFactory);

        var variables = new global::TeaPie.Variables.Variables();
        var variablesResolver = new VariablesResolver(variables, serviceProvider);
        var headersResolver = new HeadersHandler();

        return new HttpRequestParser(
            headersProvider, variablesResolver, headersResolver, Substitute.For<IRetryingHandler>());
    }
}
