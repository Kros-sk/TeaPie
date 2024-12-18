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
        ValidateContext(out var requestExecutionContext, out var request);

        var response = await ExecuteRequest(context, request, cancellationToken);

        requestExecutionContext.Response = response;
        requestExecutionContext.TestCaseExecutionContext?.RegisterResponse(response, requestExecutionContext.Name);
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

    private void ValidateContext(out RequestExecutionContext requestExecutionContext, out HttpRequestMessage request)
    {
        const string activityName = "execute request";
        ExecutionContextValidator.Validate(_requestExecutionContextAccessor, out requestExecutionContext, activityName);
        ExecutionContextValidator.ValidateParameter(
            requestExecutionContext.Request, out request, activityName, "request message");
    }
}
