using static TeaPie.Testing.XunitAssertExtensions;
using static Xunit.Assert;

namespace TeaPie.Tests.Testing;

public class XunitAssertExtensionsShould
{
    #region GreaterThan

    [Theory]
    [InlineData(5, 3)]
    [InlineData(0, -1)]
    [InlineData(int.MaxValue, int.MinValue)]
    public void EvaluateGreaterThanAssertionCorrectlyForIntegers(int value, int limit)
        => GreaterThan(value, limit);

    [Theory]
    [InlineData(3, 5)]
    [InlineData(5, 5)]
    public void ThrowOnFailedGreaterThanAssertionForIntegers(int value, int limit)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => GreaterThan(value, limit));

    [Theory]
    [InlineData(5L, 3L)]
    [InlineData(0L, -1L)]
    public void EvaluateGreaterThanAssertionCorrectlyForLongs(long value, long limit)
        => GreaterThan(value, limit);

    [Theory]
    [InlineData(5.0, 3.0, 0.001)]
    [InlineData(5.1, 5.0, 0.05)]
    public void EvaluateGreaterThanAssertionCorrectlyForDoublesWithEpsilon(double value, double limit, double epsilon)
        => GreaterThan(value, limit, epsilon);

    [Theory]
    [InlineData(3.0, 5.0, 0.001)]
    [InlineData(5.0, 5.0, 0.001)]
    [InlineData(5.0005, 5.0, 0.001)]
    public void ThrowOnFailedGreaterThanAssertionForDoublesWithEpsilon(double value, double limit, double epsilon)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => GreaterThan(value, limit, epsilon));

    [Theory]
    [InlineData(5.0f, 3.0f, 0.001f)]
    [InlineData(5.1f, 5.0f, 0.05f)]
    public void EvaluateGreaterThanAssertionCorrectlyForFloatsWithEpsilon(float value, float limit, float epsilon)
        => GreaterThan(value, limit, epsilon);

    [Theory]
    [InlineData(3.0f, 5.0f, 0.001f)]
    [InlineData(5.0f, 5.0f, 0.001f)]
    public void ThrowOnFailedGreaterThanAssertionForFloatsWithEpsilon(float value, float limit, float epsilon)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => GreaterThan(value, limit, epsilon));

    #endregion

    #region GreaterThanOrEqual

    [Theory]
    [InlineData(5, 3)]
    [InlineData(5, 5)]
    [InlineData(0, -1)]
    public void EvaluateGreaterThanOrEqualAssertionCorrectlyForIntegers(int value, int limit)
        => GreaterThanOrEqual(value, limit);

    [Theory]
    [InlineData(3, 5)]
    [InlineData(-1, 0)]
    public void ThrowOnFailedGreaterThanOrEqualAssertionForIntegers(int value, int limit)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => GreaterThanOrEqual(value, limit));

    [Theory]
    [InlineData(5.0, 3.0, 0.001)]
    [InlineData(5.0, 5.0, 0.001)]
    [InlineData(5.0005, 5.0, 0.001)]
    public void EvaluateGreaterThanOrEqualAssertionCorrectlyForDoublesWithEpsilon(double value, double limit, double epsilon)
        => GreaterThanOrEqual(value, limit, epsilon);

    [Theory]
    [InlineData(3.0, 5.0, 0.001)]
    [InlineData(4.998, 5.0, 0.001)]
    public void ThrowOnFailedGreaterThanOrEqualAssertionForDoublesWithEpsilon(double value, double limit, double epsilon)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => GreaterThanOrEqual(value, limit, epsilon));

    [Theory]
    [InlineData(5.0f, 3.0f, 0.001f)]
    [InlineData(5.0f, 5.0f, 0.001f)]
    public void EvaluateGreaterThanOrEqualAssertionCorrectlyForFloatsWithEpsilon(float value, float limit, float epsilon)
        => GreaterThanOrEqual(value, limit, epsilon);

    [Theory]
    [InlineData(3.0f, 5.0f, 0.001f)]
    public void ThrowOnFailedGreaterThanOrEqualAssertionForFloatsWithEpsilon(float value, float limit, float epsilon)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => GreaterThanOrEqual(value, limit, epsilon));

    #endregion

    #region LessThan

    [Theory]
    [InlineData(3, 5)]
    [InlineData(-1, 0)]
    [InlineData(int.MinValue, int.MaxValue)]
    public void EvaluateLessThanAssertionCorrectlyForIntegers(int value, int limit)
        => LessThan(value, limit);

    [Theory]
    [InlineData(5, 3)]
    [InlineData(5, 5)]
    public void ThrowOnFailedLessThanAssertionForIntegers(int value, int limit)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => LessThan(value, limit));

    [Theory]
    [InlineData(3L, 5L)]
    [InlineData(-1L, 0L)]
    public void EvaluateLessThanAssertionCorrectlyForLongs(long value, long limit)
        => LessThan(value, limit);

    [Theory]
    [InlineData(3.0, 5.0, 0.001)]
    [InlineData(4.9, 5.0, 0.05)]
    public void EvaluateLessThanAssertionCorrectlyForDoublesWithEpsilon(double value, double limit, double epsilon)
        => LessThan(value, limit, epsilon);

    [Theory]
    [InlineData(5.0, 3.0, 0.001)]
    [InlineData(5.0, 5.0, 0.001)]
    [InlineData(4.9995, 5.0, 0.001)]
    public void ThrowOnFailedLessThanAssertionForDoublesWithEpsilon(double value, double limit, double epsilon)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => LessThan(value, limit, epsilon));

    [Theory]
    [InlineData(3.0f, 5.0f, 0.001f)]
    [InlineData(4.9f, 5.0f, 0.05f)]
    public void EvaluateLessThanAssertionCorrectlyForFloatsWithEpsilon(float value, float limit, float epsilon)
        => LessThan(value, limit, epsilon);

    [Theory]
    [InlineData(5.0f, 3.0f, 0.001f)]
    [InlineData(5.0f, 5.0f, 0.001f)]
    public void ThrowOnFailedLessThanAssertionForFloatsWithEpsilon(float value, float limit, float epsilon)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => LessThan(value, limit, epsilon));

    #endregion

    #region LessThanOrEqual

    [Theory]
    [InlineData(3, 5)]
    [InlineData(5, 5)]
    [InlineData(-1, 0)]
    public void EvaluateLessThanOrEqualAssertionCorrectlyForIntegers(int value, int limit)
        => LessThanOrEqual(value, limit);

    [Theory]
    [InlineData(5, 3)]
    [InlineData(1, 0)]
    public void ThrowOnFailedLessThanOrEqualAssertionForIntegers(int value, int limit)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => LessThanOrEqual(value, limit));

    [Theory]
    [InlineData(3.0, 5.0, 0.001)]
    [InlineData(5.0, 5.0, 0.001)]
    [InlineData(4.9995, 5.0, 0.001)]
    public void EvaluateLessThanOrEqualAssertionCorrectlyForDoublesWithEpsilon(double value, double limit, double epsilon)
        => LessThanOrEqual(value, limit, epsilon);

    [Theory]
    [InlineData(5.0, 3.0, 0.001)]
    [InlineData(5.002, 5.0, 0.001)]
    public void ThrowOnFailedLessThanOrEqualAssertionForDoublesWithEpsilon(double value, double limit, double epsilon)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => LessThanOrEqual(value, limit, epsilon));

    [Theory]
    [InlineData(3.0f, 5.0f, 0.001f)]
    [InlineData(5.0f, 5.0f, 0.001f)]
    public void EvaluateLessThanOrEqualAssertionCorrectlyForFloatsWithEpsilon(float value, float limit, float epsilon)
        => LessThanOrEqual(value, limit, epsilon);

    [Theory]
    [InlineData(5.0f, 3.0f, 0.001f)]
    public void ThrowOnFailedLessThanOrEqualAssertionForFloatsWithEpsilon(float value, float limit, float epsilon)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => LessThanOrEqual(value, limit, epsilon));

    #endregion

    #region NullOrEmpty

    [Fact]
    public void EvaluateNullOrEmptyAssertionCorrectlyForNullString()
        => NullOrEmpty((string?)null);

    [Fact]
    public void EvaluateNullOrEmptyAssertionCorrectlyForEmptyString()
        => NullOrEmpty(string.Empty);

    [Fact]
    public void ThrowOnFailedNullOrEmptyAssertionForNonEmptyString()
        => ThrowsAny<Xunit.Sdk.XunitException>(() => NullOrEmpty("hello"));

    [Fact]
    public void EvaluateNullOrEmptyAssertionCorrectlyForNullCollection()
        => NullOrEmpty((IEnumerable<int>?)null);

    [Fact]
    public void EvaluateNullOrEmptyAssertionCorrectlyForEmptyCollection()
        => NullOrEmpty(Array.Empty<int>());

    [Fact]
    public void ThrowOnFailedNullOrEmptyAssertionForNonEmptyCollection()
        => ThrowsAny<Xunit.Sdk.XunitException>(() => NullOrEmpty(new[] { 1, 2, 3 }));

    #endregion

    #region NotNullOrEmpty

    [Fact]
    public void EvaluateNotNullOrEmptyAssertionCorrectlyForNonEmptyString()
        => NotNullOrEmpty("hello");

    [Fact]
    public void ThrowOnFailedNotNullOrEmptyAssertionForNullString()
        => ThrowsAny<Xunit.Sdk.XunitException>(() => NotNullOrEmpty((string?)null));

    [Fact]
    public void ThrowOnFailedNotNullOrEmptyAssertionForEmptyString()
        => ThrowsAny<Xunit.Sdk.XunitException>(() => NotNullOrEmpty(string.Empty));

    [Fact]
    public void EvaluateNotNullOrEmptyAssertionCorrectlyForNonEmptyCollection()
        => NotNullOrEmpty(new[] { 1, 2, 3 });

    [Fact]
    public void ThrowOnFailedNotNullOrEmptyAssertionForNullCollection()
        => ThrowsAny<Xunit.Sdk.XunitException>(() => NotNullOrEmpty((IEnumerable<int>?)null));

    [Fact]
    public void ThrowOnFailedNotNullOrEmptyAssertionForEmptyCollection()
        => ThrowsAny<Xunit.Sdk.XunitException>(() => NotNullOrEmpty(Array.Empty<int>()));

    #endregion
}
