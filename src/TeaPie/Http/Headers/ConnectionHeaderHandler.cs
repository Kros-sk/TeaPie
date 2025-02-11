using System.Net.Http.Headers;

namespace TeaPie.Http.Headers;

internal class ConnectionHeaderHandler : IHeaderHandler
{
    public string HeaderName => "Connection";

    public void SetHeader(string value, HttpRequestMessage requestMessage)
    {
        foreach (var val in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (val.Equals("close", StringComparison.OrdinalIgnoreCase))
            {
                requestMessage.Headers.ConnectionClose = true;
            }
            else if (!requestMessage.Headers.TryAddWithoutValidation(HeaderName, val))
            {
                throw new InvalidOperationException($"Unable to resolve header '{HeaderName}' with value '{val}'.");
            }
        }
    }

    public string GetHeader(HttpRequestMessage requestMessage)
        => GetConnectionHeader(requestMessage.Headers, requestMessage.Headers.ConnectionClose);

    public string GetHeader(HttpResponseMessage responseMessage)
        => GetConnectionHeader(responseMessage.Headers, responseMessage.Headers.ConnectionClose);

    private string GetConnectionHeader(HttpHeaders headers, bool? connectionClose)
    {
        if (connectionClose is null)
        {
            return string.Empty;
        }

        var values = new List<string>();

        if (headers.TryGetValues(HeaderName, out var connectionValues))
        {
            values.AddRange(connectionValues);
        }

        if (connectionClose.Value)
        {
            values.Add("close");
        }

        return string.Join(", ", values);
    }
}
