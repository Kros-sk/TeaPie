namespace TeaPie.Testing;

internal class Test(string name, Func<Task> function)
{
    public bool Executed { get; set; }
    public string Name { get; set; } = name;
    public TestResult Result { get; set; } = new(name);
    public Func<Task> Function { get; set; } = function;
}
