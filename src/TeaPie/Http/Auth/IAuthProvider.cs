namespace TeaPie.Http.Auth;

public interface IAuthProvider
{
    Task Authenticate(HttpRequestMessage request, CancellationToken cancellationToken);
}

public interface IAuthProvider<TOptions> : IAuthProvider
    where TOptions : IAuthOptions
{
    IAuthProvider<TOptions> ConfigureOptions(TOptions options);
}
