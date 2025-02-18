using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using TeaPie.Http;
using TeaPie.Http.Headers;
using TeaPie.Http.Parsing;
using TeaPie.Http.Retrying;
using TeaPie.TestCases;
using TeaPie.Variables;

namespace TeaPie.Tests.Http;

public class ExecuteRequestStepShould
{
    private const string Path = "https://jsonplaceholder.typicode.com/posts";
    private static readonly HttpMethod _method = HttpMethod.Post;
    private const HttpStatusCode StatusCode = HttpStatusCode.Created;
    private const string Body = "{\r\n  \"title\": \"foo\",\r\n  \"body\": \"bar\",\r\n  \"userId\": 1\r\n}";
    private const string MediaType = "application/json";

    [Fact]
    public async Task ThrowProperExceptionWhenRequestContextIsWithoutRequestMessage()
    {
        var serviceProvider = ConfigureServicesAndGetProvider();

        var context = RequestHelper.PrepareRequestContext(RequestsIndex.RequestWithFullStructure);

        var appContext = new ApplicationContextBuilder()
            .WithPath(RequestsIndex.RootFolderFullPath)
            .Build();

        var accessor = new RequestExecutionContextAccessor() { Context = context };

        var step = new ExecuteRequestStep(
            serviceProvider.GetRequiredService<IHttpClientFactory>(), accessor, Substitute.For<IHeadersHandler>());

        await step.Invoking(async step => await step.Execute(appContext)).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AssignCorrectResponseAfterRequestExecution()
    {
        var serviceProvider = ConfigureServicesAndGetProvider();

        var context = RequestHelper.PrepareRequestContext(RequestsIndex.RequestWithFullStructure);

        var appContext = new ApplicationContextBuilder()
            .WithPath(RequestsIndex.RootFolderFullPath)
            .Build();

        var accessor = new RequestExecutionContextAccessor() { Context = context };

        var parser = CreateParser(serviceProvider);
        parser.Parse(context);

        var step = new ExecuteRequestStep(
            serviceProvider.GetRequiredService<IHttpClientFactory>(), accessor, Substitute.For<IHeadersHandler>());

        await step.Execute(appContext);

        context.Response.Should().NotBeNull();
        context.Response!.StatusCode.Should().Be(StatusCode);

        var responseBody = await context.Response!.Content.ReadAsStringAsync();
        responseBody.Should().Be(Body);

        context.Response.Content.Headers.Should().NotBeEmpty();

        context.Response.RequestMessage.Should().NotBeNull();
        context.Response!.RequestMessage!.RequestUri.Should().BeEquivalentTo(new Uri(Path));
    }

    [Fact]
    public async Task AddResponseOfNamedRequestToParentTestCaseResponses()
    {
        const string RequestName = "FullyStructuredRequest";
        var serviceProvider = ConfigureServicesAndGetProvider();

        var testCaseContext = new TestCaseExecutionContext(null!);
        var context = RequestHelper.PrepareRequestContext(RequestsIndex.RequestWithFullStructure);
        context.TestCaseExecutionContext = testCaseContext;

        var appContext = new ApplicationContextBuilder()
            .WithPath(RequestsIndex.RootFolderFullPath)
            .Build();

        appContext.CurrentTestCase = testCaseContext;

        var accessor = new RequestExecutionContextAccessor() { Context = context };

        var parser = CreateParser(serviceProvider);
        parser.Parse(context);

        var step = new ExecuteRequestStep(
            serviceProvider.GetRequiredService<IHttpClientFactory>(), accessor, Substitute.For<IHeadersHandler>());

        await step.Execute(appContext);

        testCaseContext.Response.Should().Be(context.Response);
        testCaseContext.Responses.ContainsKey(RequestName).Should().BeTrue();
    }

    private static CustomHttpMessageHandler CreateAndConfigureMessageHandler()
        => new(request =>
        {
            if (request.Method == _method && request.RequestUri?.Equals(Path) is not null)
            {
                var response = new HttpResponseMessage
                {
                    StatusCode = StatusCode,
                    Content = new StringContent(Body),
                    RequestMessage = request
                };

                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(MediaType);
                return response;
            }

            throw new InvalidOperationException("Unsupported request.");
        });

    private static ServiceProvider ConfigureServicesAndGetProvider()
    {
        var services = new ServiceCollection();
        services.AddHttpClient();
        services.AddHttpClient<ExecuteRequestStep>().ConfigurePrimaryHttpMessageHandler(_ => CreateAndConfigureMessageHandler());

        return services.BuildServiceProvider();
    }

    private static HttpRequestParser CreateParser(IServiceProvider serviceProvider)
    {
        var clientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var headersProvider = new HttpRequestHeadersProvider(clientFactory);
        var variables = new global::TeaPie.Variables.Variables();
        var variablesResolver = new VariablesResolver(variables, serviceProvider);
        var headersResolver = new HeadersHandler();
        var retryStrategiesRegistry = new RetryStrategiesRegistry();
        var retryingHandler = new RetryingHandler(retryStrategiesRegistry, Substitute.For<ILogger<RetryingHandler>>());

        return new HttpRequestParser(
            headersProvider, variablesResolver, headersResolver, retryingHandler);
    }

    private class CustomHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseGenerator) : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseGenerator = responseGenerator;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responseGenerator(request));
        }
    }
}
