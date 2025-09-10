using Microsoft.Extensions.Logging;
using Polly;
using TeaPie.Http.Auth;
using TeaPie.Http.Headers;
using TeaPie.Pipelines;
using TeaPie.Testing;
using TeaPie.Logging;
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

    private static readonly HttpRequestOptionsKey<RequestExecutionContext> _contextKey = new("__TeaPie_Context__");

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(out var requestExecutionContext, out var request, out var resiliencePipeline);

        request.Options.Set(_contextKey, requestExecutionContext);

        var response = await ExecuteHttpRequest(context, requestExecutionContext, resiliencePipeline, request, cancellationToken);

        requestExecutionContext.Response = response;
        requestExecutionContext.TestCaseExecutionContext?.RegisterResponse(response, requestExecutionContext.Name);
    }

    private async Task<HttpResponseMessage> ExecuteHttpRequest(
        ApplicationContext context,
        RequestExecutionContext requestExecutionContext,
        ResiliencePipeline<HttpResponseMessage> resiliencePipeline,
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteRequest(requestExecutionContext, resiliencePipeline, request, cancellationToken);
        InsertStepForScheduledTestsIfAny(context);
        return response;
    }

    private async Task<HttpResponseMessage> ExecuteRequest(
        RequestExecutionContext requestExecutionContext,
        ResiliencePipeline<HttpResponseMessage> resiliencePipeline,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ResolveAuthProvider(requestExecutionContext);

        var client = _clientFactory.CreateClient(nameof(ExecuteRequestStep));
        var response = await ExecuteRequestWithRetries(requestExecutionContext, resiliencePipeline, request, client, cancellationToken);

        _authProviderAccessor.SetCurrentProviderToDefault();
        return response;
    }

    private async Task<HttpResponseMessage> ExecuteRequestWithRetries(
        RequestExecutionContext requestExecutionContext,
        ResiliencePipeline<HttpResponseMessage> resiliencePipeline,
        HttpRequestMessage request,
        HttpClient client,
        CancellationToken cancellationToken)
    {
        var originalMessage = request;
        var content = originalMessage.Content is not null
            ? await originalMessage.Content.ReadAsStringAsync(cancellationToken)
            : string.Empty;
        var messageUsed = false;

        var result = await resiliencePipeline.ExecuteAsync(async token =>
        {
            var requestToSend = GetMessage(requestExecutionContext, originalMessage, content, ref messageUsed);
            return await client.SendAsync(requestToSend, token);
        }, cancellationToken);

        RequestsLoggingHandler.LogCompletedRequest(originalMessage, result);

        return result;
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

    private HttpRequestMessage CloneMessage(HttpRequestMessage originalMessage, string content)
    {
        var request = new HttpRequestMessage(originalMessage.Method, originalMessage.RequestUri)
        {
            Content = new StringContent(content)
        };

        _headersHandler.SetHeaders(originalMessage, request);
        foreach (var option in originalMessage.Options)
        {
            request.Options.TryAdd(option.Key, option.Value);
        }
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
