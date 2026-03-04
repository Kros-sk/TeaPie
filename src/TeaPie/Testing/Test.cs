using TeaPie.StructureExploration;

namespace TeaPie.Testing;

internal record Test(string Name, bool SkipTest, Func<Task> Function, TestResult Result, TestCase? TestCase);
