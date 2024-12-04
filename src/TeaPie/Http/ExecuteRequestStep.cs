using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;

namespace TeaPie.Http;

internal class ExecuteRequestStep(IHttpClientFactory clientFactory, IRequestExecutionContextAccessor contextAccessor)
    : IPipelineStep
{
    private readonly IHttpClientFactory _clientFactory = clientFactory;
    private readonly IRequestExecutionContextAccessor _requestExecutionContextAccessor = contextAccessor;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        var requestExecutionContext = _requestExecutionContextAccessor.RequestExecutionContext
            ?? throw new NullReferenceException("Request's execution context is null.");

        var request = requestExecutionContext.Request ??
            throw new InvalidOperationException("Unable to execute request if request message is null.");

        var response = await ExecuteRequest(context, request, cancellationToken);

        requestExecutionContext.Response = response;

        UpdateCurrentTestCase(context, requestExecutionContext, response);
    }

    private async Task<HttpResponseMessage> ExecuteRequest(
        ApplicationContext context,
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        context.Logger.LogTrace("HTTP Request for '{RequestUri}' is going to be sent.", request!.RequestUri);

        var client = _clientFactory.CreateClient(nameof(ExecuteRequestStep));
        var response = await client.SendAsync(request, cancellationToken);

        context.Logger.LogTrace("HTTP Response {StatusCode} ({ReasonPhrase}) was received from '{Uri}'.",
            (int)response.StatusCode, response.ReasonPhrase, response.RequestMessage?.RequestUri);

        return response;
    }

    private static void UpdateCurrentTestCase(
        ApplicationContext context,
        RequestExecutionContext requestExecutionContext,
        HttpResponseMessage response)
    {
        if (context.CurrentTestCase is not null)
        {
            context.CurrentTestCase.Response = response;

            if (!requestExecutionContext.Name.Equals(string.Empty))
            {
                context.CurrentTestCase.Responses.Add(requestExecutionContext.Name, response);
            }
        }
    }
}
