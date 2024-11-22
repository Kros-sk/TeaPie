namespace TeaPie.Variables;

internal class Variable(string name, object? value, params string[] tags)
{
    public string Name { get; set; } = name;
    public object? Value { get; set; } = value;

    private readonly List<string> _tags = [.. tags];

    public T? GetValue<T>() => (T?)Value;
    public bool HasTag(string tag) => _tags.Contains(tag);
}
