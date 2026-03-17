// The Xunit namespace is used intentionally to extend Xunit.Assert with new members via C# 14 extension types.
#pragma warning disable IDE0130
namespace Xunit;

/// <summary>
/// Extends <see cref="Assert"/> with additional assertion methods.
/// </summary>
public static class AssertExtensions
{
    extension(Assert)
    {
        /// <summary>
        /// Verifies that <paramref name="value"/> is greater than <paramref name="limit"/>.
        /// This method works for any type implementing <see cref="IComparable{T}"/> interface.
        /// </summary>
        /// <typeparam name="T">The type of the values being compared. Must implement <see cref="IComparable{T}"/>.</typeparam>
        /// <param name="limit">The threshold value.</param>
        /// <param name="value">The value expected to be greater than <paramref name="limit"/>.</param>
        public static void GreaterThan<T>(T limit, T value) where T : IComparable<T>
        {
            if (value.CompareTo(limit) <= 0)
            {
                Assert.Fail($"Expected value to be greater than {limit}, but found {value}.");
            }
        }

        /// <summary>
        /// Verifies that <paramref name="value"/> is greater than <paramref name="limit"/>,
        /// allowing for floating-point imprecision within <paramref name="epsilon"/>.
        /// </summary>
        /// <param name="limit">The threshold value.</param>
        /// <param name="value">The value expected to be greater than <paramref name="limit"/>.</param>
        /// <param name="epsilon">The maximum allowed difference for values to be considered equal.</param>
        public static void GreaterThan(double limit, double value, double epsilon)
        {
            if (value - limit <= epsilon)
            {
                Assert.Fail($"Expected value to be greater than {limit} (epsilon: {epsilon}), but found {value}.");
            }
        }

        /// <summary>
        /// Verifies that <paramref name="value"/> is greater than <paramref name="limit"/>,
        /// allowing for floating-point imprecision within <paramref name="epsilon"/>.
        /// </summary>
        /// <param name="limit">The threshold value.</param>
        /// <param name="value">The value expected to be greater than <paramref name="limit"/>.</param>
        /// <param name="epsilon">The maximum allowed difference for values to be considered equal.</param>
        public static void GreaterThan(float limit, float value, float epsilon)
        {
            if (value - limit <= epsilon)
            {
                Assert.Fail($"Expected value to be greater than {limit} (epsilon: {epsilon}), but found {value}.");
            }
        }

        /// <summary>
        /// Verifies that <paramref name="value"/> is greater than or equal to <paramref name="limit"/>.
        /// This method works for any type implementing <see cref="IComparable{T}"/> interface.
        /// </summary>
        /// <typeparam name="T">The type of the values being compared. Must implement <see cref="IComparable{T}"/>.</typeparam>
        /// <param name="limit">The threshold value.</param>
        /// <param name="value">The value expected to be greater than or equal to <paramref name="limit"/>.</param>
        public static void GreaterThanOrEqual<T>(T limit, T value) where T : IComparable<T>
        {
            if (value.CompareTo(limit) < 0)
            {
                Assert.Fail($"Expected value to be greater than or equal to {limit}, but found {value}.");
            }
        }

        /// <summary>
        /// Verifies that <paramref name="value"/> is greater than or equal to <paramref name="limit"/>,
        /// allowing for floating-point imprecision within <paramref name="epsilon"/>.
        /// </summary>
        /// <param name="limit">The threshold value.</param>
        /// <param name="value">The value expected to be greater than or equal to <paramref name="limit"/>.</param>
        /// <param name="epsilon">The maximum allowed difference for values to be considered equal.</param>
        public static void GreaterThanOrEqual(double limit, double value, double epsilon)
        {
            if (value - limit < -epsilon)
            {
                Assert.Fail($"Expected value to be greater than or equal to {limit} (epsilon: {epsilon}), but found {value}.");
            }
        }

        /// <summary>
        /// Verifies that <paramref name="value"/> is greater than or equal to <paramref name="limit"/>,
        /// allowing for floating-point imprecision within <paramref name="epsilon"/>.
        /// </summary>
        /// <param name="limit">The threshold value.</param>
        /// <param name="value">The value expected to be greater than or equal to <paramref name="limit"/>.</param>
        /// <param name="epsilon">The maximum allowed difference for values to be considered equal.</param>
        public static void GreaterThanOrEqual(float limit, float value, float epsilon)
        {
            if (value - limit < -epsilon)
            {
                Assert.Fail($"Expected value to be greater than or equal to {limit} (epsilon: {epsilon}), but found {value}.");
            }
        }

        /// <summary>
        /// Verifies that <paramref name="value"/> is less than <paramref name="limit"/>.
        /// This method works for any type implementing <see cref="IComparable{T}"/> interface.
        /// </summary>
        /// <typeparam name="T">The type of the values being compared. Must implement <see cref="IComparable{T}"/>.</typeparam>
        /// <param name="limit">The threshold value.</param>
        /// <param name="value">The value expected to be less than <paramref name="limit"/>.</param>
        public static void LessThan<T>(T limit, T value) where T : IComparable<T>
        {
            if (value.CompareTo(limit) >= 0)
            {
                Assert.Fail($"Expected value to be less than {limit}, but found {value}.");
            }
        }

        /// <summary>
        /// Verifies that <paramref name="value"/> is less than <paramref name="limit"/>,
        /// allowing for floating-point imprecision within <paramref name="epsilon"/>.
        /// </summary>
        /// <param name="limit">The threshold value.</param>
        /// <param name="value">The value expected to be less than <paramref name="limit"/>.</param>
        /// <param name="epsilon">The maximum allowed difference for values to be considered equal.</param>
        public static void LessThan(double limit, double value, double epsilon)
        {
            if (limit - value <= epsilon)
            {
                Assert.Fail($"Expected value to be less than {limit} (epsilon: {epsilon}), but found {value}.");
            }
        }

        /// <summary>
        /// Verifies that <paramref name="value"/> is less than <paramref name="limit"/>,
        /// allowing for floating-point imprecision within <paramref name="epsilon"/>.
        /// </summary>
        /// <param name="limit">The threshold value.</param>
        /// <param name="value">The value expected to be less than <paramref name="limit"/>.</param>
        /// <param name="epsilon">The maximum allowed difference for values to be considered equal.</param>
        public static void LessThan(float limit, float value, float epsilon)
        {
            if (limit - value <= epsilon)
            {
                Assert.Fail($"Expected value to be less than {limit} (epsilon: {epsilon}), but found {value}.");
            }
        }

        /// <summary>
        /// Verifies that <paramref name="value"/> is less than or equal to <paramref name="limit"/>.
        /// This method works for any type implementing <see cref="IComparable{T}"/> interface.
        /// </summary>
        /// <typeparam name="T">The type of the values being compared. Must implement <see cref="IComparable{T}"/>.</typeparam>
        /// <param name="limit">The threshold value.</param>
        /// <param name="value">The value expected to be less than or equal to <paramref name="limit"/>.</param>
        public static void LessThanOrEqual<T>(T limit, T value) where T : IComparable<T>
        {
            if (value.CompareTo(limit) > 0)
            {
                Assert.Fail($"Expected value to be less than or equal to {limit}, but found {value}.");
            }
        }

        /// <summary>
        /// Verifies that <paramref name="value"/> is less than or equal to <paramref name="limit"/>,
        /// allowing for floating-point imprecision within <paramref name="epsilon"/>.
        /// </summary>
        /// <param name="limit">The threshold value.</param>
        /// <param name="value">The value expected to be less than or equal to <paramref name="limit"/>.</param>
        /// <param name="epsilon">The maximum allowed difference for values to be considered equal.</param>
        public static void LessThanOrEqual(double limit, double value, double epsilon)
        {
            if (value - limit > epsilon)
            {
                Assert.Fail($"Expected value to be less than or equal to {limit} (epsilon: {epsilon}), but found {value}.");
            }
        }

        /// <summary>
        /// Verifies that <paramref name="value"/> is less than or equal to <paramref name="limit"/>,
        /// allowing for floating-point imprecision within <paramref name="epsilon"/>.
        /// </summary>
        /// <param name="limit">The threshold value.</param>
        /// <param name="value">The value expected to be less than or equal to <paramref name="limit"/>.</param>
        /// <param name="epsilon">The maximum allowed difference for values to be considered equal.</param>
        public static void LessThanOrEqual(float limit, float value, float epsilon)
        {
            if (value - limit > epsilon)
            {
                Assert.Fail($"Expected value to be less than or equal to {limit} (epsilon: {epsilon}), but found {value}.");
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
                Assert.Fail($"Expected string to be null or empty, but found \"{value}\".");
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
                Assert.Fail("Expected collection to be null or empty, but it contained elements.");
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
                Assert.Fail("Expected string to be non-null and non-empty, but it was null or empty.");
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
                Assert.Fail("Expected collection to be non-null and non-empty, but it was null or empty.");
            }
        }
    }
}
