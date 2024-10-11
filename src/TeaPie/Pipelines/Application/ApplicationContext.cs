using TeaPie.StructureExploration;

namespace TeaPie.Pipelines.Application;

internal class ApplicationContext(string path)
{
    public string Path { get; set; } = path;
    public Dictionary<string, TestCase> TestCases { get; set; } = [];
}
