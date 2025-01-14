using FluentAssertions.Collections;
using FluentAssertions.Numeric;
using FluentAssertions.Primitives;

namespace TeaPie.Testing;

public static class FluentAssertionsHelperMethods
{
    public static ObjectAssertions ObjectShould(object actualValue)
        => new(actualValue);

    public static BooleanAssertions BoolShould(bool actualValue)
        => new(actualValue);

    public static NullableBooleanAssertions BoolShould(bool? actualValue)
        => new(actualValue);

    public static StringAssertions StringShould(string actualValue)
        => new(actualValue);

    public static GuidAssertions GuidShould(Guid actualValue)
        => new(actualValue);

    public static NullableGuidAssertions GuidShould(Guid? actualValue)
        => new(actualValue);

    public static GenericCollectionAssertions<T> CollectionShould<T>(IEnumerable<T> actualValue)
        => new(actualValue);

    public static ComparableTypeAssertions<T> ComparableShould<T>(IComparable<T> actualValue)
        => new(actualValue);

    #region Date and Time
    public static DateTimeAssertions DateTimeShould(DateTime actualValue)
        => new(actualValue);

    public static NullableDateTimeAssertions DateTimeShould(DateTime? actualValue)
        => new(actualValue);

    public static DateTimeOffsetAssertions DateTimeOffsetShould(DateTimeOffset actualValue)
        => new(actualValue);

    public static NullableDateTimeOffsetAssertions DateTimeOffsetShould(DateTimeOffset? actualValue)
        => new(actualValue);

    public static DateOnlyAssertions DateOnlyShould(DateOnly actualValue)
        => new(actualValue);

    public static NullableDateOnlyAssertions DateOnlyShould(DateOnly? actualValue)
        => new(actualValue);

    public static TimeOnlyAssertions TimeOnlyShould(TimeOnly actualValue)
        => new(actualValue);

    public static NullableTimeOnlyAssertions TimeOnlyShould(TimeOnly? actualValue)
        => new(actualValue);

    public static SimpleTimeSpanAssertions TimeSpanShould(TimeSpan actualValue)
        => new(actualValue);

    public static NullableSimpleTimeSpanAssertions TimeSpanShould(TimeSpan? actualValue)
        => new(actualValue);
    #endregion

    #region Numbers
    public static NumericAssertions<int> IntShould(int actualValue)
        => new(actualValue);

    public static NullableNumericAssertions<int> IntShould(int? actualValue)
        => new(actualValue);

    public static NumericAssertions<decimal> DecimalShould(decimal actualValue)
        => new(actualValue);

    public static NullableNumericAssertions<decimal> DecimalShould(decimal? actualValue)
        => new(actualValue);

    public static NumericAssertions<byte> ByteShould(byte actualValue)
        => new(actualValue);

    public static NullableNumericAssertions<byte> ByteShould(byte? actualValue)
        => new(actualValue);

    public static NumericAssertions<long> LongShould(long actualValue)
        => new(actualValue);

    public static NullableNumericAssertions<long> LongShould(long? actualValue)
        => new(actualValue);

    public static NumericAssertions<double> DoubleShould(double actualValue)
        => new(actualValue);

    public static NullableNumericAssertions<double> DoubleShould(double? actualValue)
        => new(actualValue);
    #endregion
}
