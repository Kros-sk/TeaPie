using Microsoft.Extensions.Logging;
using TeaPie.Pipelines.Application;
using TeaPie.Requests;

namespace TeaPie.Pipelines.Requests;

internal class ExecuteRequestStep(IRequestSender client, IRequestExecutionContextAccessor contextAccessor) : IPipelineStep
{
    private readonly IRequestSender _client = client;
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

        var response = await _client.SendRequest(request, cancellationToken);

        context.Logger.LogTrace("HTTP Response {StatusCode} ({ReasonPhrase}) was received from '{Uri}'.",
            (int)response.StatusCode, response.ReasonPhrase, response.RequestMessage?.RequestUri);
    }
}
