namespace TeaPie.Variables;

public class Variable(string name, object? value, params string[] tags)
{
    public string Name { get; set; } = name;
    internal object? Value { get; set; } = value;

    private readonly HashSet<string> _tags = [.. tags];
    internal bool HasTag(string tag) => _tags.Contains(tag);

    public T? GetValue<T>()
    {
        if (Value is null)
        {
            return default;
        }

        var requestedType = typeof(T);
        var actualType = Value.GetType();

        if (requestedType == actualType)
        {
            return (T?)Value;
        }

        if (IsNumericType(requestedType) && IsNumericType(actualType))
        {
            return (T?)Convert.ChangeType(Value, requestedType);
        }

        return TryGetValue<T>(requestedType, actualType);
    }

    private T? TryGetValue<T>(Type requestedType, Type actualType)
    {
        try
        {
            return (T?)Value;
        }
        catch (Exception)
        {
            throw new InvalidCastException($"Unable to cast variable of type '{actualType}' to type '{requestedType}'");
        }
    }

    private static bool IsNumericType(Type type)
        => type == typeof(int) || type == typeof(long) ||
            type == typeof(float) || type == typeof(double) ||
            type == typeof(decimal) || type == typeof(byte) ||
            type == typeof(short) || type == typeof(uint) ||
            type == typeof(ulong) || type == typeof(sbyte) ||
            type == typeof(ushort);
}
