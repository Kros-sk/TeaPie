namespace TeaPie.Http.Headers;

internal class DateHeaderHandler : IHeaderHandler
{
    public string HeaderName => "Date";

    public void SetHeader(string value, HttpRequestMessage requestMessage)
    {
        if (DateTimeOffset.TryParse(value, out var parsedDate))
        {
            requestMessage.Headers.Date = parsedDate;
        }
        else
        {
            throw new InvalidOperationException($"Invalid format for '{HeaderName}' header value.");
        }
    }

    public string GetHeader(HttpRequestMessage requestMessage)
        => requestMessage.Headers.Date?.ToString() ?? string.Empty;

    public string GetHeader(HttpResponseMessage responseMessage)
        => responseMessage.Headers.Date?.ToString() ?? string.Empty;
}
