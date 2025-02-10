using Microsoft.Extensions.Logging;
using Polly;
using TeaPie.Http.Headers;
using TeaPie.Pipelines;

namespace TeaPie.Http;

internal class ExecuteRequestStep(
    IHttpClientFactory clientFactory, IRequestExecutionContextAccessor contextAccessor, IHeadersHandler headersHandler)
    : IPipelineStep
{
    private readonly IHttpClientFactory _clientFactory = clientFactory;
    private readonly IRequestExecutionContextAccessor _requestExecutionContextAccessor = contextAccessor;
    private readonly IHeadersHandler _headersHandler = headersHandler;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(out var requestExecutionContext, out var request);

        var response = await ExecuteRequest(context, requestExecutionContext, request, cancellationToken);

        requestExecutionContext.Response = response;
        requestExecutionContext.TestCaseExecutionContext?.RegisterResponse(response, requestExecutionContext.Name);
    }

    private async Task<HttpResponseMessage> ExecuteRequest(
        ApplicationContext context,
        RequestExecutionContext requestExecutionContext,
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        context.Logger.LogTrace("HTTP Request for '{RequestUri}' is going to be sent.", request!.RequestUri);

        var client = _clientFactory.CreateClient(nameof(ExecuteRequestStep));
        var response = await ExecuteRequest(requestExecutionContext, request, client, cancellationToken);

        await LogResponse(context.Logger, response);

        return response;
    }

    private async Task<HttpResponseMessage> ExecuteRequest(
        RequestExecutionContext requestExecutionContext,
        HttpRequestMessage request,
        HttpClient client,
        CancellationToken cancellationToken)
        => requestExecutionContext.ResiliencePipeline is not null
            ? await ExecuteWithResiliencePipeline(
                requestExecutionContext, requestExecutionContext.ResiliencePipeline, request, client, cancellationToken)
            : await client.SendAsync(request, cancellationToken);

    private async Task<HttpResponseMessage> ExecuteWithResiliencePipeline(
        RequestExecutionContext requestExecutionContext,
        ResiliencePipeline resiliencePipeline,
        HttpRequestMessage request,
        HttpClient client,
        CancellationToken cancellationToken)
    {
        var originalMessage = request;
        var content = originalMessage.Content is not null
            ? await originalMessage.Content.ReadAsStringAsync(cancellationToken)
            : string.Empty;
        var messageUsed = false;

        return await resiliencePipeline.ExecuteAsync(async token =>
        {
            var request = GetMessage(requestExecutionContext, originalMessage, content, ref messageUsed);

            return await client.SendAsync(request, token);
        }, cancellationToken);
    }

    private HttpRequestMessage GetMessage(
        RequestExecutionContext requestExecutionContext,
        HttpRequestMessage originalMessage,
        string content,
        ref bool messageUsed)
    {
        var request = originalMessage;
        if (!messageUsed)
        {
            messageUsed = true;
        }
        else
        {
            request = CloneMessage(originalMessage, content);
            requestExecutionContext.Request = request;
        }

        return request;
    }

    private HttpRequestMessage CloneMessage(
        HttpRequestMessage originalMessage,
        string content)
    {
        var request = new HttpRequestMessage(originalMessage.Method, originalMessage.RequestUri)
        {
            Content = new StringContent(content)
        };

        _headersHandler.SetHeaders(originalMessage, request);
        return request;
    }

    private static async Task LogResponse(ILogger logger, HttpResponseMessage response)
    {
        logger.LogTrace("HTTP Response {StatusCode} ({ReasonPhrase}) was received from '{Uri}'.",
            (int)response.StatusCode, response.ReasonPhrase, response.RequestMessage?.RequestUri);

        logger.LogTrace("Body: {NewLine}{BodyContent}", Environment.NewLine, await response.GetBodyAsync());
    }

    private void ValidateContext(out RequestExecutionContext requestExecutionContext, out HttpRequestMessage request)
    {
        const string activityName = "execute request";
        ExecutionContextValidator.Validate(_requestExecutionContextAccessor, out requestExecutionContext, activityName);
        ExecutionContextValidator.ValidateParameter(
            requestExecutionContext.Request, out request, activityName, "request message");
    }
}
