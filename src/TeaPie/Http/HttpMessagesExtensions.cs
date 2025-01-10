namespace TeaPie.Http;

public static class HttpMessagesExtensions
{
    public static string GetBody(this HttpRequestMessage request)
        => request.Content is null ? string.Empty : request.Content.ReadAsStringAsync().Result;

    public static string GetBody(this HttpResponseMessage response)
        => response.Content is null ? string.Empty : response.Content.ReadAsStringAsync().Result;

    public static async Task<string> GetBodyAsync(this HttpRequestMessage request)
        => request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync();

    public static async Task<string> GetBodyAsync(this HttpResponseMessage response)
        => response.Content is null ? string.Empty : await response.Content.ReadAsStringAsync();
}
