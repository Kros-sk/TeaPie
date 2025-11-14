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

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Options.TryGetValue(_contextKey, out var requestContext) || requestContext is null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var attemptStartTime = DateTime.UtcNow;
        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            await LogRequestAsync(requestContext, request, response, null, attemptStartTime);
            return response;
        }
        catch (Exception ex)
        {
            await LogRequestAsync(requestContext, request, null, ex, attemptStartTime);
            throw;
        }
    }

    private async Task LogRequestAsync(
        RequestExecutionContext requestContext,
        HttpRequestMessage request,
        HttpResponseMessage? response,
        Exception? exception,
        DateTime attemptStartTime)
    {
        var logEntry = new RequestLogFileEntry
        {
            RequestId = Guid.NewGuid().ToString(),
            StartTime = attemptStartTime,
            EndTime = DateTime.UtcNow,
            Request = await CreateRequestInfoAsync(requestContext, request),
            Response = response != null ? await CreateResponseInfoAsync(response) : null,
            Authentication = CreateAuthInfo(),
            Errors = exception != null ? [exception.Message] : []
        };

        _logger.LogInformation("{@RequestLogFileEntry}", logEntry);
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
        catch(OperationCanceledException ex)
        {
            return ($"Content reading failed: {ex.Message}");
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

    private static Dictionary<string, string> ProcessHeaders(HttpHeaders headers)
    {
        return headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value));
    }
}
