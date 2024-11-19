using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using TeaPie.Extensions;
using TeaPie.Parsing;
using TeaPie.Pipelines.Application;
using TeaPie.Pipelines.Requests;
using TeaPie.Requests;
using TeaPie.StructureExploration.IO;
using TeaPie.Tests.Requests;
using File = TeaPie.StructureExploration.IO.File;

namespace TeaPie.Tests.Pipelines.Requests;

public class ExecuteRequestStepShould
{
    private const string Path = "https://jsonplaceholder.typicode.com/posts";
    private static readonly HttpMethod _method = HttpMethod.Post;
    private const HttpStatusCode StatusCode = HttpStatusCode.Created;
    private const string Body = "{\r\n  \"title\": \"foo\",\r\n  \"body\": \"bar\",\r\n  \"userId\": 1\r\n}";
    private const string MediaType = "application/json";

    [Fact]
    public async Task ResponseAfterRequestExecutionHasToBeCorrect()
    {
        var serviceProvider = ConfigureServicesAndGetProvider();

        var context = PrepareContext(RequestsIndex.RequestWithCommentsBodyAndHeadersPath);

        var appContext = new ApplicationContext(RequestsIndex.RootFolderFullPath, Substitute.For<ILogger>(), Substitute.For<IServiceProvider>());
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

    private static HttpMessageHandler CreateAndConfigureMessageHandler()
        => new FakeHttpMessageHandler(request =>
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

            throw new InvalidOperationException("Unacceptable request.");
        });

    private static IServiceProvider ConfigureServicesAndGetProvider()
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

        return new HttpFileParser(headersProvider);
    }

    private static RequestExecutionContext PrepareContext(string path)
    {
        var folder = new Folder(RequestsIndex.RootFolderFullPath, RequestsIndex.RootFolderRelativePath, RequestsIndex.RootFolderName, null);
        var file = new File(
            path,
            RequestsIndex.RootFolderFullPath.TrimRootPath(Environment.CurrentDirectory),
            System.IO.Path.GetFileName(path),
            folder);

        return new RequestExecutionContext(file)
        {
            RawContent = System.IO.File.ReadAllText(path)
        };
    }

    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseGenerator;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseGenerator)
        {
            _responseGenerator = responseGenerator;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responseGenerator(request));
        }
    }
}
