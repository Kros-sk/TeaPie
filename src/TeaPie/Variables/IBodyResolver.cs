namespace TeaPie.Variables;

internal interface IBodyResolver
{
    bool CanResolve(string mediaType);

    string Resolve(string body, string query, string defaultValue = "");
}
