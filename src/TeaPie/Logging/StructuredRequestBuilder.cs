using Serilog;
using System.Net.Http.Headers;
using TeaPie.Http;
using TeaPie.Http.Auth;

namespace TeaPie.Logging;

internal static class StructuredRequestBuilder
{
    public static async Task<StructuredRequestLog> CreateAsync(
        RequestExecutionContext requestExecutionContext,
        HttpRequestMessage request,
        IAuthProviderAccessor authProviderAccessor,
        CancellationToken cancellationToken = default)
    {
        return new StructuredRequestLog
        {
            Request = await CreateRequestInfoAsync(requestExecutionContext, request, cancellationToken),
            Authentication = CreateAuthInfo(authProviderAccessor),
            Metadata = CreateMetadata(requestExecutionContext)
        };
    }

    public static async Task LogCompletedRequestAsync(
        StructuredRequestLog structuredLog,
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        structuredLog.EndTime = DateTime.UtcNow;
        structuredLog.Response = await CreateResponseInfoAsync(response, cancellationToken);

        Log.ForContext("StructuredRequest", true)
           .Information("Request completed {@StructuredRequestLog}", structuredLog);
    }

    public static RetryAttempt CreateRetryAttempt(int attemptNumber, DateTime startTime, string reason)
    {
        return new RetryAttempt
        {
            AttemptNumber = attemptNumber,
            Timestamp = startTime,
            Reason = reason
        };
    }

    public static void FinalizeRetryAttempt(RetryAttempt attempt, HttpResponseMessage? response, Exception? exception, DateTime startTime)
    {
        attempt.IsSuccessful = response?.IsSuccessStatusCode ?? false;
        attempt.DurationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
        attempt.Exception = exception;
    }

    public static async Task<ResponseInfo> CreateResponseInfoAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        return new ResponseInfo
        {
            StatusCode = (int)response.StatusCode,
            ReasonPhrase = response.ReasonPhrase,
            Headers = ProcessHeaders(response.Headers),
            Body = response.Content != null ? await response.Content.ReadAsStringAsync(cancellationToken) : null,
            ContentType = response.Content?.Headers.ContentType?.MediaType,
            ReceivedAt = DateTime.UtcNow
        };
    }

    public static Dictionary<string, string> ProcessHeaders(HttpHeaders headers)
    {
        return headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value));
    }

    private static async Task<RequestInfo> CreateRequestInfoAsync(
        RequestExecutionContext requestExecutionContext,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return new RequestInfo
        {
            Name = requestExecutionContext.Name,
            Method = request.Method.ToString(),
            Uri = request.RequestUri?.ToString() ?? string.Empty,
            Headers = ProcessHeaders(request.Headers),
            Body = request.Content != null ? await request.Content.ReadAsStringAsync(cancellationToken) : null,
            ContentType = request.Content?.Headers.ContentType?.MediaType,
            FilePath = requestExecutionContext.RequestFile.RelativePath
        };
    }

    private static Dictionary<string, object> CreateMetadata(RequestExecutionContext requestExecutionContext)
    {
        return new Dictionary<string, object>
        {
            ["testCaseId"] = requestExecutionContext.TestCaseExecutionContext?.Id.ToString() ?? "none",
            ["hasResiliencePipeline"] = requestExecutionContext.ResiliencePipeline != null
        };
    }

    private static AuthInfo? CreateAuthInfo(IAuthProviderAccessor authProviderAccessor)
    {
        var currentProvider = authProviderAccessor.CurrentProvider;
        return currentProvider == null
            ? null
            : new AuthInfo
            {
                ProviderType = currentProvider.GetType().Name,
                IsDefault = currentProvider == authProviderAccessor.DefaultProvider,
                AuthenticatedAt = DateTime.UtcNow
            };
    }
}
