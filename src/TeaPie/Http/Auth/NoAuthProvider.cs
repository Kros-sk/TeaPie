namespace TeaPie.Http.Auth;

internal class NoAuthProvider : IAuthProvider
{
    public async Task Authenticate(HttpRequestMessage request, CancellationToken cancellationToken)
        => await Task.CompletedTask;
}
