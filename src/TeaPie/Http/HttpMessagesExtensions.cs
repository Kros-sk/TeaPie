namespace TeaPie.Http;

public static class HttpMessagesExtensions
{
    /// <summary>
    /// Gets the body content as a <see cref="string"/> from the specified <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The HTTP request message to extract the body content from.</param>
    /// <returns>The body content as a <see cref="string"/>. Returns an <b>empty string</b> if the content is
    /// <see langword="null"/>.</returns>
    public static string GetBody(this HttpRequestMessage request)
        => GetBody(request.Content).Result;

    /// <summary>
    /// Gets the body content as a <see cref="string"/> from the specified <paramref name="response"/>.
    /// </summary>
    /// <param name="response">The HTTP response message to extract the body content from.</param>
    /// <returns>The body content as a <see cref="string"/>. Returns an <b>empty string</b> if the content is
    /// <see langword="null"/>.</returns>
    public static string GetBody(this HttpResponseMessage response)
        => GetBody(response.Content).Result;

    /// <summary>
    /// Asynchronously gets the body content as a <see cref="string"/> from the specified <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The HTTP request message to extract the body content from.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The result is the body content as a
    /// <see cref="string"/>. Returns an <b>empty string</b> if the content is <see langword="null"/>.</returns>
    public static async Task<string> GetBodyAsync(this HttpRequestMessage request)
        => await GetBody(request.Content);

    /// <summary>
    /// Asynchronously gets the body content as a <see cref="string"/> from the specified <paramref name="response"/>.
    /// </summary>
    /// <param name="response">The HTTP response message to extract the body content from.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The result is the body content as a
    /// <see cref="string"/>. Returns an <b>empty string</b> if the content is <see langword="null"/>.</returns>
    public static async Task<string> GetBodyAsync(this HttpResponseMessage response)
        => await GetBody(response.Content);

    private static async Task<string> GetBody(HttpContent? content)
        => content is null ? string.Empty : await content.ReadAsStringAsync();

    /// <summary>
    /// Gets the status code as an <see cref="int"/> from the specified <paramref name="response"/>.
    /// </summary>
    /// <param name="response">The HTTP response to extract the status code from.</param>
    /// <returns>The status code as an <see cref="int"/>.</returns>
    public static int StatusCode(this HttpResponseMessage response)
        => (int)response.StatusCode;
}
