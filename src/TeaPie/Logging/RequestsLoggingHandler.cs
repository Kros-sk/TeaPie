using System.Net.Http.Headers;
using TeaPie.Http;
using TeaPie.Http.Auth;
using Microsoft.Extensions.Logging;

namespace TeaPie.Logging;

internal class RequestsLoggingHandler(IAuthProviderAccessor authProviderAccessor, ILoggerFactory loggerFactory) : DelegatingHandler
{
    private readonly IAuthProviderAccessor _authProviderAccessor = authProviderAccessor;
    private readonly ILogger _logger = loggerFactory.CreateLogger("HttpRequests");
    private static readonly HttpRequestOptionsKey<RequestExecutionContext> _contextKey = new("__TeaPie_Context__");
    public static readonly HttpRequestOptionsKey<RequestLogFileEntry> LogEntryKey = new("__TeaPie_LogEntry__");

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Options.TryGetValue(_contextKey, out var requestContext) || requestContext is null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var logEntry = await GetOrCreateLogEntryAsync(request, requestContext);
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
            await RecordAttemptAsync(logEntry, response, exception, attemptStartTime);
            await LogCompletedRequestAsync(request, response);
        }
    }

    private async Task<RequestLogFileEntry> GetOrCreateLogEntryAsync(HttpRequestMessage request, RequestExecutionContext requestContext)
    {
        if (request.Options.TryGetValue(LogEntryKey, out var existingEntry) && existingEntry != null)
        {
            return existingEntry;
        }

        var logEntry = new RequestLogFileEntry
        {
            RequestId = Guid.NewGuid().ToString(),
            StartTime = DateTime.UtcNow,
            Request = await CreateRequestInfoAsync(requestContext, request),
            Authentication = CreateAuthInfo(),
            Metadata = CreateMetadata(requestContext),
            Retries = new RetryInfo { AttemptCount = 0, Attempts = [] },
            Errors = []
        };

        request.Options.Set(LogEntryKey, logEntry);
        return logEntry;
    }

    private static async Task RecordAttemptAsync(RequestLogFileEntry logEntry, HttpResponseMessage? response, Exception? exception, DateTime attemptStartTime)
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
            attempt.Response = await CreateResponseInfoAsync(response);
        }

        logEntry.Retries.Attempts.Add(attempt);
        logEntry.Retries.AttemptCount = attemptNumber;
        logEntry.EndTime = DateTime.UtcNow;

        if (exception != null)
        {
            logEntry.Errors.Add(exception.Message);
        }
    }

    public async Task LogCompletedRequestAsync(HttpRequestMessage request, HttpResponseMessage? finalResponse)
    {
        if (request.Options.TryGetValue(LogEntryKey, out var logEntry) && logEntry != null)
        {
            if (finalResponse != null && logEntry.Response == null)
            {
                logEntry.Response = await CreateResponseInfoAsync(finalResponse);
            }

            _logger.LogInformation("{@RequestLogFileEntry}", logEntry);
        }
    }

    private static async Task<RequestInfo> CreateRequestInfoAsync(RequestExecutionContext requestContext, HttpRequestMessage request)
    {
        return new RequestInfo
        {
            Name = requestContext.Name,
            Method = request.Method.ToString(),
            Uri = request.RequestUri?.ToString() ?? string.Empty,
            Headers = ProcessHeaders(request.Headers),
            Body = await GetContentBodyAsync(request.Content),
            ContentType = request.Content?.Headers.ContentType?.MediaType,
            FilePath = requestContext.RequestFile.RelativePath
        };
    }

    private static async Task<ResponseInfo> CreateResponseInfoAsync(HttpResponseMessage response)
    {
        return new ResponseInfo
        {
            StatusCode = (int)response.StatusCode,
            ReasonPhrase = response.ReasonPhrase,
            Headers = ProcessHeaders(response.Headers),
            Body = await GetContentBodyAsync(response.Content),
            ContentType = response.Content?.Headers.ContentType?.MediaType,
            ReceivedAt = DateTime.UtcNow
        };
    }

    private static async Task<string?> GetContentBodyAsync(HttpContent? content)
    {
        if (content == null)
        {
            return null;
        }

        try
        {
            return await content.ReadAsStringAsync();
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
