namespace TeaPie.Variables;

public class Variable(string name, object? value, params string[] tags)
{
    public string Name { get; set; } = name;
    internal object? Value { get; set; } = value;

    private readonly HashSet<string> _tags = [.. tags];

    public T? GetValue<T>() => (T?)Value;
    internal bool HasTag(string tag) => _tags.Contains(tag);
}
