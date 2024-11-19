using Microsoft.Extensions.Logging;
using TeaPie.Pipelines.Application;

namespace TeaPie.Pipelines.Requests;

internal class ExecuteRequestStep(IHttpClientFactory clientFactory, IRequestExecutionContextAccessor contextAccessor)
    : IPipelineStep
{
    private readonly IHttpClientFactory _clientFactory = clientFactory;
    private readonly IRequestExecutionContextAccessor _requestExecutionContextAccessor = contextAccessor;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        var requestExecutionContext = _requestExecutionContextAccessor.RequestExecutionContext
            ?? throw new NullReferenceException("Request's execution context is null.");

        if (requestExecutionContext.RequestMessage is null)
        {
            throw new InvalidOperationException("Request message is null.");
        }

        var request = requestExecutionContext.RequestMessage;

        context.Logger.LogTrace("HTTP Request for '{RequestUri}' is going to be sent.", request.RequestUri);

        using var client = _clientFactory.CreateClient(nameof(ExecuteRequestStep));

        var response = await client.SendAsync(request, cancellationToken);

        context.Logger.LogTrace("HTTP Response {StatusCode} ({ReasonPhrase}) was received from '{Uri}'.",
            (int)response.StatusCode, response.ReasonPhrase, response.RequestMessage?.RequestUri);
    }
}
