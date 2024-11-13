using TeaPie.Parsing;

namespace TeaPie.Requests;

internal class Client(HttpClient client)
{
    private readonly HttpClient _client = client;

    public async Task SendRequest(HttpRequestStructure structure)
    {
        var response = await _client.SendAsync(structure.Message);
        Console.WriteLine(response);
    }
}
