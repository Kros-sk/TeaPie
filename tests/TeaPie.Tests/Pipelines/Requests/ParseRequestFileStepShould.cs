using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Parsing;
using TeaPie.Pipelines.Application;
using TeaPie.Pipelines.Requests;
using TeaPie.Requests;
using TeaPie.Tests.Requests;

namespace TeaPie.Tests.Pipelines.Requests;

public class ParseRequestFileStepShould
{
    [Fact]
    public async Task RequestContextWithoutRawContentShouldThrowProperException()
    {
        var context = RequestHelper.PrepareContext(RequestsIndex.RequestWithCommentsBodyAndHeadersPath, false);

        var appContext = new ApplicationContext(
            RequestsIndex.RootFolderFullPath,
            Substitute.For<ILogger>(),
            Substitute.For<IServiceProvider>());

        var accessor = new RequestExecutionContextAccessor() { RequestExecutionContext = context };

        var parser = CreateParser();
        var step = new ParseRequestFileStep(accessor, parser);

        await step.Invoking(async step => await step.Execute(appContext)).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RequestMessageShouldBeAssignedCorrectly()
    {
        var context = RequestHelper.PrepareContext(RequestsIndex.RequestWithCommentsBodyAndHeadersPath);

        var appContext = new ApplicationContext(
            RequestsIndex.RootFolderFullPath,
            Substitute.For<ILogger>(),
            Substitute.For<IServiceProvider>());

        var accessor = new RequestExecutionContextAccessor() { RequestExecutionContext = context };

        var parser = CreateParser();
        var step = new ParseRequestFileStep(accessor, parser);

        await step.Execute(appContext);

        context.Request.Should().NotBeNull();
        context.Request!.Method.Should().Be(HttpMethod.Post);
        context.Request!.Content.Should().NotBeNull();
        context.Request!.Headers.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ParseMethodOnParserShouldBeCalled()
    {
        var context = RequestHelper.PrepareContext(RequestsIndex.PlainGetRequestPath);

        var appContext = new ApplicationContext(
            RequestsIndex.RootFolderFullPath,
            Substitute.For<ILogger>(),
            Substitute.For<IServiceProvider>());

        var accessor = new RequestExecutionContextAccessor() { RequestExecutionContext = context };

        var parser = Substitute.For<IHttpFileParser>();
        var step = new ParseRequestFileStep(accessor, parser);

        await step.Execute(appContext);

        parser.Received(1).Parse(context.RawContent!);
    }

    private static HttpFileParser CreateParser()
    {
        var services = new ServiceCollection();
        services.AddHttpClient();

        var serviceProvider = services.BuildServiceProvider();

        var clientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var headersProvider = new HttpRequestHeadersProvider(clientFactory);

        return new HttpFileParser(headersProvider);
    }
}
