using System.Text.Json;
using System.Text.Json.Serialization;
using TeaPie.Json;
using Serializer = System.Text.Json.JsonSerializer;

namespace TeaPie.Http;

public static class HttpMessagesExtensions
{
    private static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    #region Synchronous methods
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
    /// Gets the body content as a <typeparamref name="TResult"/> from the specified <paramref name="request"/>.
    /// This is possible only if the body content is in JSON structure.
    /// </summary>
    /// <typeparam name="TResult">Type which JSON body content will be deserialized to.</typeparam>
    /// <param name="request">The HTTP request message to extract the body content from.</param>
    /// <returns>The body content as a <typeparamref name="TResult"/>.</returns>
    public static TResult? GetBody<TResult>(this HttpRequestMessage request)
        => Serializer.Deserialize<TResult>(GetBody(request.Content).Result);

    /// <summary>
    /// Gets the body content as a <typeparamref name="TResult"/> from the specified <paramref name="response"/>.
    /// This is possible only if the body content is in JSON structure.
    /// </summary>
    /// <typeparam name="TResult">Type which JSON body content will be deserialized to.</typeparam>
    /// <param name="response">The HTTP response message to extract the body content from.</param>
    /// <returns>The body content as a <typeparamref name="TResult"/>.</returns>
    public static TResult? GetBody<TResult>(this HttpResponseMessage response)
        => Serializer.Deserialize<TResult>(GetBody(response.Content).Result);

    /// <summary>
    /// Gets the body content as a <see cref="CaseInsensitiveExpandoObject"/> from the specified <paramref name="request"/>.
    /// This is possible only if the body content is in JSON structure.
    /// </summary>
    /// <param name="request">The HTTP request message to extract the body content from.</param>
    /// <returns>The body content as a <see cref="CaseInsensitiveExpandoObject"/>.</returns>
    public static CaseInsensitiveExpandoObject GetBodyAsExpando(this HttpRequestMessage request)
        => GetBody(request.Content).Result.ToExpando();

    /// <summary>
    /// Gets the body content as a <see cref="CaseInsensitiveExpandoObject"/> from the specified <paramref name="response"/>.
    /// This is possible only if the body content is in JSON structure.
    /// </summary>
    /// <param name="response">The HTTP response message to extract the body content from.</param>
    /// <returns>The body content as a <see cref="CaseInsensitiveExpandoObject"/>.</returns>
    public static CaseInsensitiveExpandoObject GetBodyAsExpando(this HttpResponseMessage response)
        => GetBody(response.Content).Result.ToExpando();
    #endregion

    #region Asynchronous methods
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

    /// <summary>
    /// Asynchronously gets the body content as a <typeparamref name="TResult"/> from the specified <paramref name="request"/>.
    /// This is possible only if the body content is in JSON structure.
    /// </summary>
    /// <typeparam name="TResult">Type which JSON body content will be deserialized to.</typeparam>
    /// <param name="request">The HTTP request message to extract the body content from.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The result is the body content as a
    /// <typeparamref name="TResult"/>.</returns>
    public static async Task<TResult?> GetBodyAsync<TResult>(this HttpRequestMessage request)
        => Serializer.Deserialize<TResult>(await GetBody(request.Content), _jsonOptions);

    /// <summary>
    /// Asynchronously gets the body content as a <typeparamref name="TResult"/> from the specified <paramref name="response"/>.
    /// This is possible only if the body content is in JSON structure.
    /// </summary>
    /// <typeparam name="TResult">Type which JSON body content will be deserialized to.</typeparam>
    /// <param name="response">The HTTP response message to extract the body content from.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The result is the body content as a
    /// <typeparamref name="TResult"/>.</returns>
    public static async Task<TResult?> GetBodyAsync<TResult>(this HttpResponseMessage response)
        => Serializer.Deserialize<TResult>(await GetBody(response.Content), _jsonOptions);

    /// <summary>
    /// Asynchronously gets the body content as a <see cref="CaseInsensitiveExpandoObject"/>
    /// from the specified <paramref name="request"/>. This is possible only if the body content is in JSON structure.
    /// </summary>
    /// <param name="request">The HTTP request message to extract the body content from.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The result is the body content as a
    /// <see cref="CaseInsensitiveExpandoObject"/>.</returns>
    public static async Task<CaseInsensitiveExpandoObject> GetBodyAsExpandoAsync(this HttpRequestMessage request)
        => (await GetBody(request.Content)).ToExpando();

    /// <summary>
    /// Asynchronously gets the body content as a <see cref="CaseInsensitiveExpandoObject"/>
    /// from the specified <paramref name="response"/>. This is possible only if the body content is in JSON structure.
    /// </summary>
    /// <param name="response">The HTTP response message to extract the body content from.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The result is the body content as a
    /// <see cref="CaseInsensitiveExpandoObject"/>.</returns>
    public static async Task<CaseInsensitiveExpandoObject> GetBodyAsExpandoAsync(this HttpResponseMessage response)
        => (await GetBody(response.Content)).ToExpando();
    #endregion

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
