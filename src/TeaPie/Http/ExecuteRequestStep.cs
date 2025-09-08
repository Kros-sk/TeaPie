using Microsoft.Extensions.Logging;
using Polly;
using TeaPie.Http.Auth;
using TeaPie.Http.Headers;
using TeaPie.Logging;
using TeaPie.Pipelines;
using TeaPie.Testing;

namespace TeaPie.Http;

internal class ExecuteRequestStep(
    IHttpClientFactory clientFactory,
    IRequestExecutionContextAccessor contextAccessor,
    IHeadersHandler headersHandler,
    IAuthProviderAccessor defaultAuthProviderAccessor,
    ITestScheduler testScheduler,
    IPipeline pipeline)
    : IPipelineStep
{
    private readonly IHttpClientFactory _clientFactory = clientFactory;
    private readonly IRequestExecutionContextAccessor _requestExecutionContextAccessor = contextAccessor;
    private readonly IHeadersHandler _headersHandler = headersHandler;
    private readonly IAuthProviderAccessor _authProviderAccessor = defaultAuthProviderAccessor;
    private readonly IPipeline _pipeline = pipeline;
    private readonly ITestScheduler _testScheduler = testScheduler;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(out var requestExecutionContext, out var request, out var resiliencePipeline);

        var structuredLog = await StructuredRequestBuilder.CreateAsync(
            requestExecutionContext, request, _authProviderAccessor, cancellationToken);

        var response = await ExecuteHttpRequest(context, requestExecutionContext, resiliencePipeline, request, structuredLog, cancellationToken);

        requestExecutionContext.Response = response;
        requestExecutionContext.TestCaseExecutionContext?.RegisterResponse(response, requestExecutionContext.Name);

        await StructuredRequestBuilder.LogCompletedRequestAsync(structuredLog, response, cancellationToken);
    }

    private async Task<HttpResponseMessage> ExecuteHttpRequest(
        ApplicationContext context,
        RequestExecutionContext requestExecutionContext,
        ResiliencePipeline<HttpResponseMessage> resiliencePipeline,
        HttpRequestMessage request,
        StructuredRequestLog structuredLog,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteRequest(context, requestExecutionContext, resiliencePipeline, request, structuredLog, cancellationToken);
        InsertStepForScheduledTestsIfAny(context);
        return response;
    }

    private async Task<HttpResponseMessage> ExecuteRequest(
        ApplicationContext context,
        RequestExecutionContext requestExecutionContext,
        ResiliencePipeline<HttpResponseMessage> resiliencePipeline,
        HttpRequestMessage request,
        StructuredRequestLog structuredLog,
        CancellationToken cancellationToken)
    {
        ResolveAuthProvider(requestExecutionContext);

        var client = _clientFactory.CreateClient(nameof(ExecuteRequestStep));
        var response = await ExecuteRequestWithRetries(
            requestExecutionContext, resiliencePipeline, request, client, context.Logger, structuredLog, cancellationToken);

        _authProviderAccessor.SetCurrentProviderToDefault();
        return response;
    }

    private async Task<HttpResponseMessage> ExecuteRequestWithRetries(
        RequestExecutionContext requestExecutionContext,
        ResiliencePipeline<HttpResponseMessage> resiliencePipeline,
        HttpRequestMessage request,
        HttpClient client,
        Microsoft.Extensions.Logging.ILogger logger,
        StructuredRequestLog structuredLog,
        CancellationToken cancellationToken)
    {
        var originalMessage = request;
        var content = originalMessage.Content is not null
            ? await originalMessage.Content.ReadAsStringAsync(cancellationToken)
            : string.Empty;
        var messageUsed = false;
        var retryAttemptNumber = -1;

        return await resiliencePipeline.ExecuteAsync(async token =>
        {
            retryAttemptNumber++;
            var attemptStartTime = DateTime.UtcNow;
            var reason = retryAttemptNumber > 0 ? "Resilience policy triggered retry" : "Initial attempt";
            var retryAttempt = StructuredRequestBuilder.CreateRetryAttempt(retryAttemptNumber + 1, attemptStartTime, reason);

            try
            {
                if (retryAttemptNumber > 0)
                {
                    logger.LogDebug("Retry attempt number {Number}.", retryAttemptNumber);
                }

                var request = GetMessage(requestExecutionContext, originalMessage, content, ref messageUsed);
                var response = await client.SendAsync(request, token);
                retryAttempt.Response = await StructuredRequestBuilder.CreateResponseInfoAsync(response, token);
                StructuredRequestBuilder.FinalizeRetryAttempt(retryAttempt, response, null, attemptStartTime);
                structuredLog.Retries.Attempts.Add(retryAttempt);
                structuredLog.Retries.AttemptCount = retryAttemptNumber + 1;
                return response;
            }
            catch (Exception ex)
            {
                StructuredRequestBuilder.FinalizeRetryAttempt(retryAttempt, null, ex, attemptStartTime);
                structuredLog.Retries.Attempts.Add(retryAttempt);
                structuredLog.Retries.AttemptCount = retryAttemptNumber + 1;
                throw;
            }
        }, cancellationToken);
    }

    private void InsertStepForScheduledTestsIfAny(ApplicationContext context)
    {
        if (_testScheduler.HasScheduledTest())
        {
            _pipeline.InsertSteps(this, context.ServiceProvider.GetStep<ExecuteScheduledTestsStep>());
            context.Logger.LogDebug("Tests from test directives were scheduled for execution.");
        }
    }

    private void ResolveAuthProvider(RequestExecutionContext requestExecutionContext)
    {
        if (requestExecutionContext.AuthProvider is null)
        {
            _authProviderAccessor.SetCurrentProviderToDefault();
        }
        else
        {
            _authProviderAccessor.CurrentProvider = requestExecutionContext.AuthProvider;
        }
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

    private void ValidateContext(
        out RequestExecutionContext requestExecutionContext,
        out HttpRequestMessage request,
        out ResiliencePipeline<HttpResponseMessage> resiliencePipeline)
    {
        const string activityName = "execute request";
        ExecutionContextValidator.Validate(_requestExecutionContextAccessor, out requestExecutionContext, activityName);
        ExecutionContextValidator.ValidateParameter(
            requestExecutionContext.Request, out request, activityName, "request message");
        ExecutionContextValidator.ValidateParameter(
            requestExecutionContext.ResiliencePipeline, out resiliencePipeline, activityName, "resilience pipeline");
    }
}
