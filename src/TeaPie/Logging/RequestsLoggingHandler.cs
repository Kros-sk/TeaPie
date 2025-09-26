using System.Net.Http.Headers;
using TeaPie.Http;
using TeaPie.Http.Auth;
using Serilog;

namespace TeaPie.Logging;

internal class RequestsLoggingHandler(IAuthProviderAccessor authProviderAccessor) : DelegatingHandler
{
    private readonly IAuthProviderAccessor _authProviderAccessor = authProviderAccessor;
    private static readonly HttpRequestOptionsKey<RequestExecutionContext> _contextKey = new("__TeaPie_Context__");
    public static readonly HttpRequestOptionsKey<RequestLogFileEntry> LogEntryKey = new("__TeaPie_LogEntry__");

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Options.TryGetValue(_contextKey, out var requestContext) || requestContext is null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var logEntry = GetOrCreateLogEntry(request, requestContext);
        var attemptStartTime = DateTime.UtcNow;
        HttpResponseMessage? response = null;
        Exception? exception = null;

        try
        {
            response = await base.SendAsync(request, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            RecordAttempt(logEntry, response, exception, attemptStartTime);
        }
    }

    private RequestLogFileEntry GetOrCreateLogEntry(HttpRequestMessage request, RequestExecutionContext requestContext)
    {
        if (request.Options.TryGetValue(LogEntryKey, out var existingEntry) && existingEntry != null)
        {
            return existingEntry;
        }

        var logEntry = new RequestLogFileEntry
        {
            RequestId = Guid.NewGuid().ToString(),
            StartTime = DateTime.UtcNow,
            Request = CreateRequestInfo(requestContext, request),
            Authentication = CreateAuthInfo(),
            Metadata = CreateMetadata(requestContext),
            Retries = new RetryInfo { AttemptCount = 0, Attempts = [] },
            Errors = []
        };

        request.Options.Set(LogEntryKey, logEntry);
        return logEntry;
    }

    private static void RecordAttempt(RequestLogFileEntry logEntry, HttpResponseMessage? response, Exception? exception, DateTime attemptStartTime)
    {
        var attemptNumber = logEntry.Retries.AttemptCount + 1;
        var attempt = new RetryAttempt
        {
            AttemptNumber = attemptNumber,
            Timestamp = attemptStartTime,
            Reason = attemptNumber == 1 ? "Initial attempt" : "Resilience policy triggered retry",
            IsSuccessful = response?.IsSuccessStatusCode ?? false,
            DurationMs = (DateTime.UtcNow - attemptStartTime).TotalMilliseconds,
            Exception = exception
        };

        if (response != null)
        {
            attempt.Response = CreateResponseInfo(response);
        }

        logEntry.Retries.Attempts.Add(attempt);
        logEntry.Retries.AttemptCount = attemptNumber;
        logEntry.EndTime = DateTime.UtcNow;

        if (exception != null)
        {
            logEntry.Errors.Add(exception.Message);
        }
    }

    public static void LogCompletedRequest(HttpRequestMessage request, HttpResponseMessage? finalResponse)
    {
        if (request.Options.TryGetValue(LogEntryKey, out var logEntry) && logEntry != null)
        {
            if (finalResponse != null)
            {
                logEntry.Response = CreateResponseInfo(finalResponse);
            }

            Log.ForContext("SourceContext", "HttpRequests")
               .Information("{@RequestLogFileEntry}", logEntry);
        }
    }

    private static RequestInfo CreateRequestInfo(RequestExecutionContext requestContext, HttpRequestMessage request)
    {
        return new RequestInfo
        {
            Name = requestContext.Name,
            Method = request.Method.ToString(),
            Uri = request.RequestUri?.ToString() ?? string.Empty,
            Headers = ProcessHeaders(request.Headers),
            Body = GetRequestBody(request),
            ContentType = request.Content?.Headers.ContentType?.MediaType,
            FilePath = requestContext.RequestFile.RelativePath
        };
    }

    private static string? GetRequestBody(HttpRequestMessage request)
    {
        if (request.Content == null)
        {
            return null;
        }

        try
        {
            return request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }
        catch
        {
            return "[Content reading failed]";
        }
    }

    private static ResponseInfo CreateResponseInfo(HttpResponseMessage response)
    {
        return new ResponseInfo
        {
            StatusCode = (int)response.StatusCode,
            ReasonPhrase = response.ReasonPhrase,
            Headers = ProcessHeaders(response.Headers),
            Body = GetResponseBody(response),
            ContentType = response.Content?.Headers.ContentType?.MediaType,
            ReceivedAt = DateTime.UtcNow
        };
    }

    private static string? GetResponseBody(HttpResponseMessage response)
    {
        if (response.Content == null)
        {
            return null;
        }

        try
        {
            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }
        catch
        {
            return "[Content reading failed]";
        }
    }

    private AuthInfo? CreateAuthInfo()
    {
        var currentProvider = _authProviderAccessor.CurrentProvider;
        return currentProvider == null ? null : new AuthInfo
        {
            ProviderType = currentProvider.GetType().Name,
            IsDefault = currentProvider == _authProviderAccessor.DefaultProvider,
            AuthenticatedAt = DateTime.UtcNow
        };
    }

    private static Dictionary<string, object> CreateMetadata(RequestExecutionContext requestContext)
    {
        return new Dictionary<string, object>
        {
            ["testCaseId"] = requestContext.TestCaseExecutionContext?.Id.ToString() ?? "none",
            ["hasResiliencePipeline"] = requestContext.ResiliencePipeline != null
        };
    }

    private static Dictionary<string, string> ProcessHeaders(HttpHeaders headers)
    {
        return headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value));
    }
}
