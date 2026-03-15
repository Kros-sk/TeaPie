using TeaPie.Json;
using static Xunit.Assert;

namespace TeaPie.Testing;

public static class XunitAssertExtensions
{
    /// <summary>
    /// Verifies whether a JSON object (<paramref name="container"/>) contains another JSON object
    /// (<paramref name="contained"/>). If not, assertion exception is thrown.
    /// Property names specified in <paramref name="ignoreProperties"/> are excluded from the comparison.
    /// </summary>
    /// <param name="container">The JSON string expected to contain the <paramref name="contained"/> JSON.</param>
    /// <param name="contained">The JSON string expected to be contained within the <paramref name="container"/> JSON.</param>
    /// <param name="ignoreProperties">An array of property names to exclude from the comparison.</param>
    public static void JsonContains(string container, string contained, params string[] ignoreProperties)
    {
        if (!JsonHelper.JsonContains(container, contained, out var error, ignoreProperties))
        {
            Fail("The provided JSON does not contain the expected JSON." + Environment.NewLine +
                 $"Error: Expected {error.Value.expected}, but found {error.Value.found}.");
        }
    }

    /// <summary>
    /// Verifies that <paramref name="value"/> is greater than <paramref name="limit"/>.
    /// This method works for any type implementing <see cref="IComparable{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value">The value expected to be greater than <paramref name="limit"/>.</param>
    /// <param name="limit">The threshold value.</param>
    public static void GreaterThan<T>(T value, T limit) where T : IComparable<T>
    {
        if (value.CompareTo(limit) <= 0)
        {
            Fail($"Expected value to be greater than {limit}, but found {value}.");
        }
    }

    /// <summary>
    /// Verifies that <paramref name="value"/> is greater than <paramref name="limit"/>,
    /// allowing for floating-point imprecision within <paramref name="epsilon"/>.
    /// </summary>
    /// <param name="value">The value expected to be greater than <paramref name="limit"/>.</param>
    /// <param name="limit">The threshold value.</param>
    /// <param name="epsilon">The maximum allowed difference for values to be considered equal.</param>
    public static void GreaterThan(double value, double limit, double epsilon)
    {
        if (value - limit <= epsilon)
        {
            Fail($"Expected value to be greater than {limit} (epsilon: {epsilon}), but found {value}.");
        }
    }

    /// <summary>
    /// Verifies that <paramref name="value"/> is greater than <paramref name="limit"/>,
    /// allowing for floating-point imprecision within <paramref name="epsilon"/>.
    /// </summary>
    /// <param name="value">The value expected to be greater than <paramref name="limit"/>.</param>
    /// <param name="limit">The threshold value.</param>
    /// <param name="epsilon">The maximum allowed difference for values to be considered equal.</param>
    public static void GreaterThan(float value, float limit, float epsilon)
    {
        if (value - limit <= epsilon)
        {
            Fail($"Expected value to be greater than {limit} (epsilon: {epsilon}), but found {value}.");
        }
    }

    /// <summary>
    /// Verifies that <paramref name="value"/> is greater than or equal to <paramref name="limit"/>.
    /// This method works for any type implementing <see cref="IComparable{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value">The value expected to be greater than or equal to <paramref name="limit"/>.</param>
    /// <param name="limit">The threshold value.</param>
    public static void GreaterThanOrEqual<T>(T value, T limit) where T : IComparable<T>
    {
        if (value.CompareTo(limit) < 0)
        {
            Fail($"Expected value to be greater than or equal to {limit}, but found {value}.");
        }
    }

    /// <summary>
    /// Verifies that <paramref name="value"/> is greater than or equal to <paramref name="limit"/>,
    /// allowing for floating-point imprecision within <paramref name="epsilon"/>.
    /// </summary>
    /// <param name="value">The value expected to be greater than or equal to <paramref name="limit"/>.</param>
    /// <param name="limit">The threshold value.</param>
    /// <param name="epsilon">The maximum allowed difference for values to be considered equal.</param>
    public static void GreaterThanOrEqual(double value, double limit, double epsilon)
    {
        if (value - limit < -epsilon)
        {
            Fail($"Expected value to be greater than or equal to {limit} (epsilon: {epsilon}), but found {value}.");
        }
    }

    /// <summary>
    /// Verifies that <paramref name="value"/> is greater than or equal to <paramref name="limit"/>,
    /// allowing for floating-point imprecision within <paramref name="epsilon"/>.
    /// </summary>
    /// <param name="value">The value expected to be greater than or equal to <paramref name="limit"/>.</param>
    /// <param name="limit">The threshold value.</param>
    /// <param name="epsilon">The maximum allowed difference for values to be considered equal.</param>
    public static void GreaterThanOrEqual(float value, float limit, float epsilon)
    {
        if (value - limit < -epsilon)
        {
            Fail($"Expected value to be greater than or equal to {limit} (epsilon: {epsilon}), but found {value}.");
        }
    }

    /// <summary>
    /// Verifies that <paramref name="value"/> is less than <paramref name="limit"/>.
    /// This method works for any type implementing <see cref="IComparable{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value">The value expected to be less than <paramref name="limit"/>.</param>
    /// <param name="limit">The threshold value.</param>
    public static void LessThan<T>(T value, T limit) where T : IComparable<T>
    {
        if (value.CompareTo(limit) >= 0)
        {
            Fail($"Expected value to be less than {limit}, but found {value}.");
        }
    }

    /// <summary>
    /// Verifies that <paramref name="value"/> is less than <paramref name="limit"/>,
    /// allowing for floating-point imprecision within <paramref name="epsilon"/>.
    /// </summary>
    /// <param name="value">The value expected to be less than <paramref name="limit"/>.</param>
    /// <param name="limit">The threshold value.</param>
    /// <param name="epsilon">The maximum allowed difference for values to be considered equal.</param>
    public static void LessThan(double value, double limit, double epsilon)
    {
        if (limit - value <= epsilon)
        {
            Fail($"Expected value to be less than {limit} (epsilon: {epsilon}), but found {value}.");
        }
    }

    /// <summary>
    /// Verifies that <paramref name="value"/> is less than <paramref name="limit"/>,
    /// allowing for floating-point imprecision within <paramref name="epsilon"/>.
    /// </summary>
    /// <param name="value">The value expected to be less than <paramref name="limit"/>.</param>
    /// <param name="limit">The threshold value.</param>
    /// <param name="epsilon">The maximum allowed difference for values to be considered equal.</param>
    public static void LessThan(float value, float limit, float epsilon)
    {
        if (limit - value <= epsilon)
        {
            Fail($"Expected value to be less than {limit} (epsilon: {epsilon}), but found {value}.");
        }
    }

    /// <summary>
    /// Verifies that <paramref name="value"/> is less than or equal to <paramref name="limit"/>.
    /// This method works for any type implementing <see cref="IComparable{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value">The value expected to be less than or equal to <paramref name="limit"/>.</param>
    /// <param name="limit">The threshold value.</param>
    public static void LessThanOrEqual<T>(T value, T limit) where T : IComparable<T>
    {
        if (value.CompareTo(limit) > 0)
        {
            Fail($"Expected value to be less than or equal to {limit}, but found {value}.");
        }
    }

    /// <summary>
    /// Verifies that <paramref name="value"/> is less than or equal to <paramref name="limit"/>,
    /// allowing for floating-point imprecision within <paramref name="epsilon"/>.
    /// </summary>
    /// <param name="value">The value expected to be less than or equal to <paramref name="limit"/>.</param>
    /// <param name="limit">The threshold value.</param>
    /// <param name="epsilon">The maximum allowed difference for values to be considered equal.</param>
    public static void LessThanOrEqual(double value, double limit, double epsilon)
    {
        if (value - limit > epsilon)
        {
            Fail($"Expected value to be less than or equal to {limit} (epsilon: {epsilon}), but found {value}.");
        }
    }

    /// <summary>
    /// Verifies that <paramref name="value"/> is less than or equal to <paramref name="limit"/>,
    /// allowing for floating-point imprecision within <paramref name="epsilon"/>.
    /// </summary>
    /// <param name="value">The value expected to be less than or equal to <paramref name="limit"/>.</param>
    /// <param name="limit">The threshold value.</param>
    /// <param name="epsilon">The maximum allowed difference for values to be considered equal.</param>
    public static void LessThanOrEqual(float value, float limit, float epsilon)
    {
        if (value - limit > epsilon)
        {
            Fail($"Expected value to be less than or equal to {limit} (epsilon: {epsilon}), but found {value}.");
        }
    }

    /// <summary>
    /// Verifies that <paramref name="value"/> is <see langword="null"/> or empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    public static void NullOrEmpty(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            Fail($"Expected string to be null or empty, but found \"{value}\".");
        }
    }

    /// <summary>
    /// Verifies that <paramref name="collection"/> is <see langword="null"/> or empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to check.</param>
    public static void NullOrEmpty<T>(IEnumerable<T>? collection)
    {
        if (collection is not null && collection.Any())
        {
            Fail("Expected collection to be null or empty, but it contained elements.");
        }
    }

    /// <summary>
    /// Verifies that <paramref name="value"/> is not <see langword="null"/> and not empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    public static void NotNullOrEmpty(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Fail("Expected string to be non-null and non-empty, but it was null or empty.");
        }
    }

    /// <summary>
    /// Verifies that <paramref name="collection"/> is not <see langword="null"/> and not empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to check.</param>
    public static void NotNullOrEmpty<T>(IEnumerable<T>? collection)
    {
        if (collection is null || !collection.Any())
        {
            Fail("Expected collection to be non-null and non-empty, but it was null or empty.");
        }
    }
}
