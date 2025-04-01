using Microsoft.Extensions.Logging;
namespace TeaPie.Logging;

internal class LoggingInterceptorHandler(ILogger<LoggingInterceptorHandler> logger) : DelegatingHandler
{
    private readonly ILogger<LoggingInterceptorHandler> _logger = logger;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await LogRequestBody(request, cancellationToken);

        var response = await base.SendAsync(request, cancellationToken);

        await LogResponse(response, cancellationToken);

        return response;
    }

    private async Task LogRequestBody(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content is not null)
        {
            var content = await request.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogTrace("Following HTTP request's body ({ContentType}):{NewLine}{Body}",
                request.Content.Headers.ContentType?.MediaType,
                Environment.NewLine,
                content);
        }
        else
        {
            _logger.LogTrace("Following HTTP request doesn't have any body.");
        }
    }

    private async Task LogResponse(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        _logger.LogTrace("HTTP Response {StatusCode} ({ReasonPhrase}) was received from '{Uri}'.",
            (int)response.StatusCode, response.ReasonPhrase, response.RequestMessage?.RequestUri);

        if (response.Content is not null)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogTrace("Response's body ({ContentType}): {NewLine}{BodyContent}",
                response.Content.Headers.ContentType?.MediaType ?? "text",
                Environment.NewLine,
                content);
        }
        else
        {
            _logger.LogTrace("Response is without body.");
        }
    }
}
