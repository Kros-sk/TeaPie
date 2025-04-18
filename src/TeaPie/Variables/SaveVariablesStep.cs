using Microsoft.Extensions.Logging;
using System.Text.Json;
using TeaPie.Pipelines;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.Variables;

internal class SaveVariablesStep(IVariables variables, IPathProvider pathProvider) : IPipelineStep
{
    private readonly IVariables _variables = variables;
    private readonly IPathProvider _pathProvider = pathProvider;

    public bool ShouldExecute(ApplicationContext context) => context.CacheVariables;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        await SaveVariables(_pathProvider.VariablesFilePath);

        context.Logger.LogDebug("Variables from current run were saved to file.");
    }

    private async Task SaveVariables(string variablesFilePath)
    {
        var variablesByScopes = GetVariablesByScopes();
        var json = JsonSerializer.Serialize(variablesByScopes);

        await SaveToFile(variablesFilePath, json);
    }

    private static async Task SaveToFile(string variablesFilePath, string content)
    {
        CreateParentDirectoryIfNeeded(variablesFilePath);

        await WriteToFile(variablesFilePath, content);
    }

    private static async Task WriteToFile(string variablesFilePath, string content)
    {
        await using var fileStream = new FileStream(variablesFilePath, FileMode.Create, FileAccess.Write);
        await using var writer = new StreamWriter(fileStream);

        await writer.WriteAsync(content);
    }

    private static void CreateParentDirectoryIfNeeded(string variablesFilePath)
    {
        var directory = Path.GetDirectoryName(variablesFilePath);
        if (!Directory.Exists(directory) && directory is not null)
        {
            Directory.CreateDirectory(directory);
        }
    }

    private Dictionary<string, Dictionary<string, object?>> GetVariablesByScopes()
        => new()
        {
            { nameof(_variables.GlobalVariables), GetScopeVariables(_variables.GlobalVariables) },
            { nameof(_variables.EnvironmentVariables), GetScopeVariables(_variables.EnvironmentVariables) },
            { nameof(_variables.CollectionVariables), GetScopeVariables(_variables.CollectionVariables) },
            { nameof(_variables.TestCaseVariables), GetScopeVariables(_variables.TestCaseVariables) }
        };

    private static Dictionary<string, object?> GetScopeVariables(VariablesCollection variablesCollection)
    {
        var scopeVariables = new Dictionary<string, object?>();
        foreach (var variable in variablesCollection)
        {
            if (!variable.HasTag(Constants.SecretVariableTag) && !variable.HasTag(Constants.NoCacheVariableTag))
            {
                scopeVariables[variable.Name] = variable.Value;
            }
        }

        return scopeVariables;
    }
}
