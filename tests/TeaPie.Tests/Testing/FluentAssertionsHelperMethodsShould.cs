using TeaPie.Json;
using TeaPie.Tests.Json;
using static TeaPie.Testing.FluentAssertionsHelperMethods;

namespace TeaPie.Tests.Testing;

public class FluentAssertionsHelperMethodsShould
{
    [Fact]
    public void HandleDynamicInputProperly()
    {
        dynamic obj = JsonExtensionsShould.JsonString.ToJsonExpando();

        StringShould(obj.stringKey).Be(obj.stringKey);
        LongShould(obj.numberKey).Be(obj.numberKey);
        BoolShould(obj.booleanKey).Be(obj.booleanKey);
        CollectionShould(obj.arrayKey).Equal(obj.arrayKey);
        ObjectShould(obj.objectKey).Be(obj.objectKey);
        StringShould(obj.objectKey.nestedStringKey).Be(obj.objectKey.nestedStringKey);
    }

    [Fact]
    public void HandleStringProperly()
        => StringShould("string").BeEquivalentTo("string");

    [Theory]
    [MemberData(nameof(GetDummy))]
    public void HandleObjectProperly(object input)
        => ObjectShould(input).Be(input);

    [Fact]
    public void HandleBooleanProperly()
        => BoolShould(true).Be(true);

    [Theory]
    [InlineData(true)]
    [InlineData(null)]
    public void HandleNullableBooleanProperly(bool? input)
        => BoolShould(input).Be(input);

    [Fact]
    public void HandleGuidProperly()
    {
        var input = Guid.NewGuid();
        GuidShould(input).Be(input);
    }

    [Fact]
    public void HandleNullableGuidProperly()
    {
        Guid? input = Guid.NewGuid();
        GuidShould(input).Be(input);
    }

    [Fact]
    public void HandleCollectionProperly()
    {
        var input = new[] { 1, 2, 3 };
        CollectionShould(input).BeEquivalentTo(input);
    }

    [Fact]
    public void HandleComparableProperly()
    {
        var input = new ComparableDummy(42);
        ComparableShould(input).Be(input);
    }

    #region Date and Time
    [Fact]
    public void HandleDateTimeProperly()
    {
        var input = DateTime.Now;
        DateTimeShould(input).Be(input);
    }

    [Fact]
    public void HandleNullableDateTimeProperly()
    {
        DateTime? input = DateTime.Now;
        DateTimeShould(input).Be(input);
        DateTimeShould(null).BeNull();
    }

    [Fact]
    public void HandleDateTimeOffsetProperly()
    {
        var input = DateTimeOffset.Now;
        DateTimeOffsetShould(input).Be(input);
    }

    [Fact]
    public void HandleNullableDateTimeOffsetProperly()
    {
        DateTimeOffset? input = DateTimeOffset.Now;
        DateTimeOffsetShould(input).Be(input);
        DateTimeOffsetShould(null).BeNull();
    }

    [Fact]
    public void HandleDateOnlyProperly()
    {
        var input = DateOnly.FromDateTime(DateTime.Now);
        DateOnlyShould(input).Be(input);
    }

    [Fact]
    public void HandleNullableDateOnlyProperly()
    {
        DateOnly? input = DateOnly.FromDateTime(DateTime.Now);
        DateOnlyShould(input).Be(input);
        DateOnlyShould(null).BeNull();
    }

    [Fact]
    public void HandleTimeOnlyProperly()
    {
        var input = TimeOnly.FromDateTime(DateTime.Now);
        TimeOnlyShould(input).Be(input);
    }

    [Fact]
    public void HandleNullableTimeOnlyProperly()
    {
        TimeOnly? input = TimeOnly.FromDateTime(DateTime.Now);
        TimeOnlyShould(input).Be(input);
        TimeOnlyShould(null).BeNull();
    }

    [Fact]
    public void HandleTimeSpanProperly()
    {
        var input = TimeSpan.FromHours(1);
        TimeSpanShould(input).Be(input);
    }

    [Fact]
    public void HandleNullableTimeSpanProperly()
    {
        TimeSpan? input = TimeSpan.FromHours(1);
        TimeSpanShould(input).Be(input);
        TimeSpanShould(null).BeNull();
    }

    #endregion

    #region Numbers

    [Fact]
    public void HandleIntProperly()
        => IntShould(23).Be(23);

    [Theory]
    [InlineData(42)]
    [InlineData(null)]
    public void HandleNullableIntProperly(int? input)
        => IntShould(input).Be(input);

    [Fact]
    public void HandleDoubleProperly()
        => DoubleShould(23).Be(23);

    [Theory]
    [InlineData(42.42)]
    [InlineData(null)]
    public void HandleNullableDoubleProperly(double? input)
        => DoubleShould(input).Be(input);

    [Fact]
    public void HandleDecimalProperly()
        => DecimalShould(42.42m).Be(42.42m);

    [Fact]
    public void HandleNullableDecimalProperly()
    {
        DecimalShould(24.24m).Be(24.24m);
        DecimalShould(null).BeNull();
    }

    [Fact]
    public void HandleByteProperly()
        => ByteShould(42).Be(42);

    [Theory]
    [InlineData((byte)42)]
    [InlineData(null)]
    public void HandleNullableByteProperly(byte? input)
        => ByteShould(input).Be(input);

    [Fact]
    public void HandleLongProperly()
        => LongShould(42L).Be(42L);

    [Theory]
    [InlineData(42L)]
    [InlineData(null)]
    public void HandleNullableLongProperly(long? input)
        => LongShould(input).Be(input);
    #endregion

    public static IEnumerable<object[]> GetDummy() => [[new Dummy()]];

    private class Dummy;

    private class ComparableDummy(int id) : IComparable<ComparableDummy>
    {
        private readonly int _id = id;
        public int CompareTo(ComparableDummy? other) => _id.CompareTo(other?._id);
    }
}
