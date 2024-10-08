namespace TeaPie.Pipelines.Application;

internal class ApplicationContext(string path)
{
    public string Path { get; set; } = path;
}
