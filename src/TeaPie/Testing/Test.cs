using TeaPie.StructureExploration;

namespace TeaPie.Testing;

internal record Test(string Name, Func<Task> Function, TestResult Result, TestCase? TestCase);
