namespace TeaPie.StructureExploration;

internal interface IExternalFileRegistry : IRegistry<ExternalFile>;

internal class ExternalFilesRegistry : IExternalFileRegistry
{
    private readonly Dictionary<string, ExternalFile> _externalFiles = [];

    public void Register(string name, ExternalFile element) => _externalFiles[name] = element;

    public ExternalFile Get(string name) => _externalFiles[name];

    public bool IsRegistered(string name) => _externalFiles.ContainsKey(name);
}
