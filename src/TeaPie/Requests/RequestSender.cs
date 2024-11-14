namespace TeaPie.Requests;

internal interface IRequestSender
{
    Task<HttpResponseMessage> SendRequest(HttpRequestMessage message, CancellationToken cancellationToken = default);
}

internal class RequestSender(HttpClient client) : IRequestSender
{
    private readonly HttpClient _client = client;

    public async Task<HttpResponseMessage> SendRequest(HttpRequestMessage message, CancellationToken cancellationToken = default)
        => await _client.SendAsync(message, cancellationToken);
}
