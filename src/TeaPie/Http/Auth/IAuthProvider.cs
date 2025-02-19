namespace TeaPie.Http.Auth;

public interface IAuthProvider
{
    Task Authenticate(HttpRequestMessage request, CancellationToken cancellationToken);
}
