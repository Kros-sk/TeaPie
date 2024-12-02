using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using TeaPie.Http;
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

        var context = RequestHelper.PrepareContext(RequestsIndex.RequestWithCommentsBodyAndHeadersPath);

        var appContext = new ApplicationContext(
            RequestsIndex.RootFolderFullPath,
            Substitute.For<ILogger>(),
            Substitute.For<IServiceProvider>());

        var accessor = new RequestExecutionContextAccessor() { RequestExecutionContext = context };

        var step = new ExecuteRequestStep(serviceProvider.GetRequiredService<IHttpClientFactory>(), accessor);

        await step.Invoking(async step => await step.Execute(appContext)).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AssignCorrectResponseAfterRequestExecution()
    {
        var serviceProvider = ConfigureServicesAndGetProvider();

        var context = RequestHelper.PrepareContext(RequestsIndex.RequestWithCommentsBodyAndHeadersPath);

        var appContext = new ApplicationContext(
            RequestsIndex.RootFolderFullPath,
            Substitute.For<ILogger>(),
            Substitute.For<IServiceProvider>());

        var accessor = new RequestExecutionContextAccessor() { RequestExecutionContext = context };

        var parser = CreateParser(serviceProvider);
        context.Request = parser.Parse(context.RawContent!);

        var step = new ExecuteRequestStep(serviceProvider.GetRequiredService<IHttpClientFactory>(), accessor);

        await step.Execute(appContext);

        context.Response.Should().NotBeNull();
        context.Response!.StatusCode.Should().Be(StatusCode);

        var responseBody = await context.Response!.Content.ReadAsStringAsync();
        responseBody.Should().Be(Body);

        context.Response.Content.Headers.Should().NotBeEmpty();

        context.Response.RequestMessage.Should().NotBeNull();
        context.Response!.RequestMessage!.RequestUri.Should().BeEquivalentTo(new Uri(Path));
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

    private static HttpFileParser CreateParser(IServiceProvider serviceProvider)
    {
        var clientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var headersProvider = new HttpRequestHeadersProvider(clientFactory);
        var variables = new global::TeaPie.Variables.Variables();
        var variablesResolver = new VariablesResolver(variables);

        return new HttpFileParser(headersProvider, variablesResolver);
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
