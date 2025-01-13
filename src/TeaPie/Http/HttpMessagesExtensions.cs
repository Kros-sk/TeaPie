namespace TeaPie.Http;

public static class HttpMessagesExtensions
{
    /// <summary>
    /// Retrieves the <see cref="string"/> representation of the body content from the specified <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The HTTP request message whose body content should be retrieved.</param>
    /// <returns>The body content of the <paramref name="request"/> as a <see cref="string"/>.
    /// Returns an empty string if the content is null.</returns>
    public static string GetBody(this HttpRequestMessage request)
        => request.Content is null ? string.Empty : request.Content.ReadAsStringAsync().Result;

    /// <summary>
    /// Retrieves the <see cref="string"/> representation of the body content from the specified <paramref name="response"/>.
    /// </summary>
    /// <param name="response">The HTTP response message whose body content should be retrieved.</param>
    /// <returns>The body content of the <paramref name="response"/> as a <see cref="string"/>.
    /// Returns an empty string if the content is null.</returns>
    public static string GetBody(this HttpResponseMessage response)
        => response.Content is null ? string.Empty : response.Content.ReadAsStringAsync().Result;

    /// <summary>
    /// Asynchronously retrieves the <see cref="string"/> representation of the body content from the specified
    /// <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The HTTP request message whose body content should be retrieved.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the body content of the
    /// <paramref name="request"/> as a <see cref="string"/>. Returns an empty string if the content is null.</returns>
    public static async Task<string> GetBodyAsync(this HttpRequestMessage request)
        => request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync();

    /// <summary>
    /// Asynchronously retrieves the <see cref="string"/> representation of the body content from the specified
    /// <paramref name="response"/>.
    /// </summary>
    /// <param name="response">The HTTP response message whose body content should be retrieved.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the body content of the
    /// <paramref name="response"/> as a <see cref="string"/>. Returns an empty string if the content is null.</returns>
    public static async Task<string> GetBodyAsync(this HttpResponseMessage response)
        => response.Content is null ? string.Empty : await response.Content.ReadAsStringAsync();
}
