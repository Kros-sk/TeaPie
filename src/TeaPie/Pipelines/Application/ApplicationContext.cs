using TeaPie.StructureExploration;

namespace TeaPie.Pipelines.Application;

internal class ApplicationContext(string path, string tempFolder = "")
{
    public string Path { get; set; } = path;
    public string TempFolderPath { get; set; } = tempFolder;
    public IReadOnlyDictionary<string, TestCase> TestCases { get; set; } = new Dictionary<string, TestCase>();
    public Dictionary<string, Script> UserDefinedScripts { get; set; } = [];
}
