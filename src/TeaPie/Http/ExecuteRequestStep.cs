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
    IPipeline pipeline,
    IStructuredRequestLogger? structuredLogger = null)
    : IPipelineStep
{
    private readonly IHttpClientFactory _clientFactory = clientFactory;
    private readonly IRequestExecutionContextAccessor _requestExecutionContextAccessor = contextAccessor;
    private readonly IHeadersHandler _headersHandler = headersHandler;
    private readonly IAuthProviderAccessor _authProviderAccessor = defaultAuthProviderAccessor;
    private readonly IPipeline _pipeline = pipeline;
    private readonly ITestScheduler _testScheduler = testScheduler;
    private readonly IStructuredRequestLogger? _structuredLogger = structuredLogger;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(out var requestExecutionContext, out var request, out var resiliencePipeline);

        var structuredLog = await CreateStructuredLog(requestExecutionContext, request, cancellationToken);

        var response = await Execute(context, requestExecutionContext, resiliencePipeline, request, structuredLog, cancellationToken);

        requestExecutionContext.Response = response;
        requestExecutionContext.TestCaseExecutionContext?.RegisterResponse(response, requestExecutionContext.Name);

        await FinalizeStructuredLog(structuredLog, response, cancellationToken);
    }

    private AuthInfo? CreateAuthInfo()
    {
        var currentProvider = _authProviderAccessor.CurrentProvider;
        return currentProvider == null
            ? null
            : new AuthInfo
        {
            ProviderType = currentProvider.GetType().Name,
            IsDefault = currentProvider == _authProviderAccessor.DefaultProvider,
            AuthenticatedAt = DateTime.UtcNow
        };
    }

    private async Task<HttpResponseMessage> Execute(
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

    private void InsertStepForScheduledTestsIfAny(ApplicationContext context)
    {
        if (_testScheduler.HasScheduledTest())
        {
            _pipeline.InsertSteps(this, context.ServiceProvider.GetStep<ExecuteScheduledTestsStep>());
            context.Logger.LogDebug("Tests from test directives were scheduled for execution.");
        }
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
        var response = await ExecuteRequest(
            requestExecutionContext, resiliencePipeline, request, client, context.Logger, structuredLog, cancellationToken);

        _authProviderAccessor.SetCurrentProviderToDefault();
        return response;
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

    private async Task<HttpResponseMessage> ExecuteRequest(
        RequestExecutionContext requestExecutionContext,
        ResiliencePipeline<HttpResponseMessage> resiliencePipeline,
        HttpRequestMessage request,
        HttpClient client,
        ILogger logger,
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
            var retryAttempt = new RetryAttempt
            {
                AttemptNumber = retryAttemptNumber + 1,
                Timestamp = attemptStartTime,
                Reason = retryAttemptNumber > 0 ? "Resilience policy triggered retry" : "Initial attempt"
            };

            try
            {
                if (retryAttemptNumber > 0)
                {
                    logger.LogDebug("Retry attempt number {Number}.", retryAttemptNumber);
                }

                var request = GetMessage(requestExecutionContext, originalMessage, content, ref messageUsed);
                var response = await client.SendAsync(request, token);
                retryAttempt.Response = new ResponseInfo
                {
                    StatusCode = (int)response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase,
                    Headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value)),
                    Body = response.Content != null ? await response.Content.ReadAsStringAsync(token) : null,
                    ContentType = response.Content?.Headers.ContentType?.MediaType,
                    ReceivedAt = DateTime.UtcNow
                };
                retryAttempt.IsSuccessful = response.IsSuccessStatusCode;
                retryAttempt.DurationMs = (DateTime.UtcNow - attemptStartTime).TotalMilliseconds;
                structuredLog.Retries.Attempts.Add(retryAttempt);
                structuredLog.Retries.AttemptCount = retryAttemptNumber + 1;
                return response;
            }
            catch (Exception ex)
            {
                retryAttempt.Exception = ex;
                retryAttempt.IsSuccessful = false;
                retryAttempt.DurationMs = (DateTime.UtcNow - attemptStartTime).TotalMilliseconds;
                structuredLog.Retries.Attempts.Add(retryAttempt);
                structuredLog.Retries.AttemptCount = retryAttemptNumber + 1;
                throw;
            }
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

    private async Task<StructuredRequestLog> CreateStructuredLog(
    RequestExecutionContext requestExecutionContext,
    HttpRequestMessage request,
    CancellationToken cancellationToken)
    {
        var structuredLog = new StructuredRequestLog
        {
            Request = new RequestInfo
            {
                Name = requestExecutionContext.Name,
                Method = request.Method.ToString(),
                Uri = request.RequestUri?.ToString() ?? string.Empty,
                Headers = request.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value)),
                Body = request.Content != null ? await request.Content.ReadAsStringAsync(cancellationToken) : null,
                ContentType = request.Content?.Headers.ContentType?.MediaType,
                FilePath = requestExecutionContext.RequestFile.RelativePath
            },
            Authentication = CreateAuthInfo(),
            Metadata = new Dictionary<string, object>
            {
                ["testCaseId"] = requestExecutionContext.TestCaseExecutionContext?.Id.ToString() ?? "none",
                ["hasResiliencePipeline"] = requestExecutionContext.ResiliencePipeline != null
            }
        };

        return structuredLog;
    }

    private async Task FinalizeStructuredLog(
      StructuredRequestLog structuredLog,
      HttpResponseMessage response,
      CancellationToken cancellationToken)
    {
        structuredLog.EndTime = DateTime.UtcNow;
        structuredLog.Response = new ResponseInfo
        {
            StatusCode = (int)response.StatusCode,
            ReasonPhrase = response.ReasonPhrase,
            Headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value)),
            Body = response.Content != null ? await response.Content.ReadAsStringAsync(cancellationToken) : null,
            ContentType = response.Content?.Headers.ContentType?.MediaType
        };

        if (_structuredLogger != null)
        {
            await _structuredLogger.LogRequestAsync(structuredLog, cancellationToken);
        }
    }
}
