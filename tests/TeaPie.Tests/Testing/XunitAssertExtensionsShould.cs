using static Xunit.Assert;

namespace TeaPie.Tests.Testing;

public class XunitAssertExtensionsShould
{
    #region GreaterThan

    [Theory]
    [InlineData(3, 5)]
    [InlineData(-1, 0)]
    [InlineData(int.MinValue, int.MaxValue)]
    public void EvaluateGreaterThanAssertionCorrectlyForIntegers(int limit, int value)
        => Assert.GreaterThan(limit, value);

    [Theory]
    [InlineData(5, 3)]
    [InlineData(5, 5)]
    public void ThrowOnFailedGreaterThanAssertionForIntegers(int limit, int value)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => Assert.GreaterThan(limit, value));

    [Theory]
    [InlineData(3L, 5L)]
    [InlineData(-1L, 0L)]
    public void EvaluateGreaterThanAssertionCorrectlyForLongs(long limit, long value)
        => Assert.GreaterThan(limit, value);

    [Theory]
    [InlineData(3.0, 5.0, 0.001)]
    [InlineData(5.0, 5.1, 0.05)]
    public void EvaluateGreaterThanAssertionCorrectlyForDoublesWithEpsilon(double limit, double value, double epsilon)
        => Assert.GreaterThan(limit, value, epsilon);

    [Theory]
    [InlineData(5.0, 3.0, 0.001)]
    [InlineData(5.0, 5.0, 0.001)]
    [InlineData(5.0, 5.0005, 0.001)]
    public void ThrowOnFailedGreaterThanAssertionForDoublesWithEpsilon(double limit, double value, double epsilon)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => Assert.GreaterThan(limit, value, epsilon));

    [Theory]
    [InlineData(3.0f, 5.0f, 0.001f)]
    [InlineData(5.0f, 5.1f, 0.05f)]
    public void EvaluateGreaterThanAssertionCorrectlyForFloatsWithEpsilon(float limit, float value, float epsilon)
        => Assert.GreaterThan(limit, value, epsilon);

    [Theory]
    [InlineData(5.0f, 3.0f, 0.001f)]
    [InlineData(5.0f, 5.0f, 0.001f)]
    public void ThrowOnFailedGreaterThanAssertionForFloatsWithEpsilon(float limit, float value, float epsilon)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => Assert.GreaterThan(limit, value, epsilon));

    #endregion

    #region GreaterThanOrEqual

    [Theory]
    [InlineData(3, 5)]
    [InlineData(5, 5)]
    [InlineData(-1, 0)]
    public void EvaluateGreaterThanOrEqualAssertionCorrectlyForIntegers(int limit, int value)
        => Assert.GreaterThanOrEqual(limit, value);

    [Theory]
    [InlineData(5, 3)]
    [InlineData(0, -1)]
    public void ThrowOnFailedGreaterThanOrEqualAssertionForIntegers(int limit, int value)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => Assert.GreaterThanOrEqual(limit, value));

    [Theory]
    [InlineData(3.0, 5.0, 0.001)]
    [InlineData(5.0, 5.0, 0.001)]
    [InlineData(5.0, 5.0005, 0.001)]
    public void EvaluateGreaterThanOrEqualAssertionCorrectlyForDoublesWithEpsilon(double limit, double value, double epsilon)
        => Assert.GreaterThanOrEqual(limit, value, epsilon);

    [Theory]
    [InlineData(5.0, 3.0, 0.001)]
    [InlineData(5.0, 4.998, 0.001)]
    public void ThrowOnFailedGreaterThanOrEqualAssertionForDoublesWithEpsilon(double limit, double value, double epsilon)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => Assert.GreaterThanOrEqual(limit, value, epsilon));

    [Theory]
    [InlineData(3.0f, 5.0f, 0.001f)]
    [InlineData(5.0f, 5.0f, 0.001f)]
    public void EvaluateGreaterThanOrEqualAssertionCorrectlyForFloatsWithEpsilon(float limit, float value, float epsilon)
        => Assert.GreaterThanOrEqual(limit, value, epsilon);

    [Theory]
    [InlineData(5.0f, 3.0f, 0.001f)]
    public void ThrowOnFailedGreaterThanOrEqualAssertionForFloatsWithEpsilon(float limit, float value, float epsilon)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => Assert.GreaterThanOrEqual(limit, value, epsilon));

    #endregion

    #region LessThan

    [Theory]
    [InlineData(5, 3)]
    [InlineData(0, -1)]
    [InlineData(int.MaxValue, int.MinValue)]
    public void EvaluateLessThanAssertionCorrectlyForIntegers(int limit, int value)
        => Assert.LessThan(limit, value);

    [Theory]
    [InlineData(3, 5)]
    [InlineData(5, 5)]
    public void ThrowOnFailedLessThanAssertionForIntegers(int limit, int value)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => Assert.LessThan(limit, value));

    [Theory]
    [InlineData(5L, 3L)]
    [InlineData(0L, -1L)]
    public void EvaluateLessThanAssertionCorrectlyForLongs(long limit, long value)
        => Assert.LessThan(limit, value);

    [Theory]
    [InlineData(5.0, 3.0, 0.001)]
    [InlineData(5.0, 4.9, 0.05)]
    public void EvaluateLessThanAssertionCorrectlyForDoublesWithEpsilon(double limit, double value, double epsilon)
        => Assert.LessThan(limit, value, epsilon);

    [Theory]
    [InlineData(3.0, 5.0, 0.001)]
    [InlineData(5.0, 5.0, 0.001)]
    [InlineData(5.0, 4.9995, 0.001)]
    public void ThrowOnFailedLessThanAssertionForDoublesWithEpsilon(double limit, double value, double epsilon)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => Assert.LessThan(limit, value, epsilon));

    [Theory]
    [InlineData(5.0f, 3.0f, 0.001f)]
    [InlineData(5.0f, 4.9f, 0.05f)]
    public void EvaluateLessThanAssertionCorrectlyForFloatsWithEpsilon(float limit, float value, float epsilon)
        => Assert.LessThan(limit, value, epsilon);

    [Theory]
    [InlineData(3.0f, 5.0f, 0.001f)]
    [InlineData(5.0f, 5.0f, 0.001f)]
    public void ThrowOnFailedLessThanAssertionForFloatsWithEpsilon(float limit, float value, float epsilon)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => Assert.LessThan(limit, value, epsilon));

    #endregion

    #region LessThanOrEqual

    [Theory]
    [InlineData(5, 3)]
    [InlineData(5, 5)]
    [InlineData(0, -1)]
    public void EvaluateLessThanOrEqualAssertionCorrectlyForIntegers(int limit, int value)
        => Assert.LessThanOrEqual(limit, value);

    [Theory]
    [InlineData(3, 5)]
    [InlineData(0, 1)]
    public void ThrowOnFailedLessThanOrEqualAssertionForIntegers(int limit, int value)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => Assert.LessThanOrEqual(limit, value));

    [Theory]
    [InlineData(5.0, 3.0, 0.001)]
    [InlineData(5.0, 5.0, 0.001)]
    [InlineData(5.0, 4.9995, 0.001)]
    public void EvaluateLessThanOrEqualAssertionCorrectlyForDoublesWithEpsilon(double limit, double value, double epsilon)
        => Assert.LessThanOrEqual(limit, value, epsilon);

    [Theory]
    [InlineData(3.0, 5.0, 0.001)]
    [InlineData(5.0, 5.002, 0.001)]
    public void ThrowOnFailedLessThanOrEqualAssertionForDoublesWithEpsilon(double limit, double value, double epsilon)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => Assert.LessThanOrEqual(limit, value, epsilon));

    [Theory]
    [InlineData(5.0f, 3.0f, 0.001f)]
    [InlineData(5.0f, 5.0f, 0.001f)]
    public void EvaluateLessThanOrEqualAssertionCorrectlyForFloatsWithEpsilon(float limit, float value, float epsilon)
        => Assert.LessThanOrEqual(limit, value, epsilon);

    [Theory]
    [InlineData(3.0f, 5.0f, 0.001f)]
    public void ThrowOnFailedLessThanOrEqualAssertionForFloatsWithEpsilon(float limit, float value, float epsilon)
        => ThrowsAny<Xunit.Sdk.XunitException>(() => Assert.LessThanOrEqual(limit, value, epsilon));

    #endregion

    #region NullOrEmpty

    [Fact]
    public void EvaluateNullOrEmptyAssertionCorrectlyForNullString()
        => Assert.NullOrEmpty((string?)null);

    [Fact]
    public void EvaluateNullOrEmptyAssertionCorrectlyForEmptyString()
        => Assert.NullOrEmpty(string.Empty);

    [Fact]
    public void ThrowOnFailedNullOrEmptyAssertionForNonEmptyString()
        => ThrowsAny<Xunit.Sdk.XunitException>(() => Assert.NullOrEmpty("hello"));

    [Fact]
    public void EvaluateNullOrEmptyAssertionCorrectlyForNullCollection()
        => Assert.NullOrEmpty((IEnumerable<int>?)null);

    [Fact]
    public void EvaluateNullOrEmptyAssertionCorrectlyForEmptyCollection()
        => Assert.NullOrEmpty(Array.Empty<int>());

    [Fact]
    public void ThrowOnFailedNullOrEmptyAssertionForNonEmptyCollection()
        => ThrowsAny<Xunit.Sdk.XunitException>(() => Assert.NullOrEmpty(new[] { 1, 2, 3 }));

    #endregion

    #region NotNullOrEmpty

    [Fact]
    public void EvaluateNotNullOrEmptyAssertionCorrectlyForNonEmptyString()
        => Assert.NotNullOrEmpty("hello");

    [Fact]
    public void ThrowOnFailedNotNullOrEmptyAssertionForNullString()
        => ThrowsAny<Xunit.Sdk.XunitException>(() => Assert.NotNullOrEmpty((string?)null));

    [Fact]
    public void ThrowOnFailedNotNullOrEmptyAssertionForEmptyString()
        => ThrowsAny<Xunit.Sdk.XunitException>(() => Assert.NotNullOrEmpty(string.Empty));

    [Fact]
    public void EvaluateNotNullOrEmptyAssertionCorrectlyForNonEmptyCollection()
        => Assert.NotNullOrEmpty(new[] { 1, 2, 3 });

    [Fact]
    public void ThrowOnFailedNotNullOrEmptyAssertionForNullCollection()
        => ThrowsAny<Xunit.Sdk.XunitException>(() => Assert.NotNullOrEmpty((IEnumerable<int>?)null));

    [Fact]
    public void ThrowOnFailedNotNullOrEmptyAssertionForEmptyCollection()
        => ThrowsAny<Xunit.Sdk.XunitException>(() => Assert.NotNullOrEmpty(Array.Empty<int>()));

    #endregion
}
