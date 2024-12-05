using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using TeaPie.Http;
using TeaPie.Variables;

namespace TeaPie.Tests.Http;

public class ParseRequestFileStepShould
{
    [Fact]
    public async Task ThrowProperExceptionWhenRequestContextIsWithoutRawContent()
    {
        var context = RequestHelper.PrepareRequestContext(RequestsIndex.RequestWithCommentsBodyAndHeadersPath, false);

        var appContext = new ApplicationContextBuilder()
            .WithPath(RequestsIndex.RootFolderFullPath)
            .Build();

        var accessor = new RequestExecutionContextAccessor() { RequestExecutionContext = context };

        var parser = CreateParser();
        var step = new ParseHttpRequestStep(accessor, parser);

        await step.Invoking(async step => await step.Execute(appContext)).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AssignRequestMessageCorrectly()
    {
        var context = RequestHelper.PrepareRequestContext(RequestsIndex.RequestWithCommentsBodyAndHeadersPath);

        var appContext = new ApplicationContextBuilder()
            .WithPath(RequestsIndex.RootFolderFullPath)
            .Build();

        var accessor = new RequestExecutionContextAccessor() { RequestExecutionContext = context };

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

        var accessor = new RequestExecutionContextAccessor() { RequestExecutionContext = context };

        var parser = Substitute.For<IHttpFileParser>();
        var step = new ParseHttpRequestStep(accessor, parser);

        await step.Execute(appContext);

        parser.Received(1).Parse(context);
    }

    private static HttpFileParser CreateParser()
    {
        var services = new ServiceCollection();
        services.AddHttpClient();

        var serviceProvider = services.BuildServiceProvider();

        var clientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var headersProvider = new HttpRequestHeadersProvider(clientFactory);

        var variables = new global::TeaPie.Variables.Variables();
        var resolver = new VariablesResolver(variables);

        return new HttpFileParser(headersProvider, resolver);
    }
}
