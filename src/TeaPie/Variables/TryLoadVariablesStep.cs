using Microsoft.Extensions.Logging;
using System.Text.Json;
using TeaPie.Json;
using TeaPie.Pipelines;
using TeaPie.StructureExploration;

namespace TeaPie.Variables;

internal class TryLoadVariablesStep(IVariables variables, IPathProvider pathProvider) : IPipelineStep
{
    private readonly IVariables _variables = variables;
    private readonly IPathProvider _pathProvider = pathProvider;

    private static readonly Lazy<JsonSerializerOptions> _jsonSerializerOptions = new(() =>
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonElementTypeConverter());
        return options;
    });

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        var variablesFilePath = _pathProvider.VariablesFilePath;
        if (Directory.Exists(variablesFilePath))
        {
            await LoadVariables(variablesFilePath);
            context.Logger.LogDebug("Variables were loaded from previous run.");
        }
        else
        {
            context.Logger.LogDebug("No variables were loaded from previous run.");
        }
    }

    private async Task LoadVariables(string variablesFilePath)
    {
        var variablesByScopes = await ParseVariablesFile(variablesFilePath);
        LoadVariables(variablesByScopes);
    }

    private static async Task<Dictionary<string, Dictionary<string, object?>>> ParseVariablesFile(string variablesFilePath)
    {
        await using var environmentFile = System.IO.File.OpenRead(variablesFilePath);
        return JsonSerializer
            .Deserialize<Dictionary<string, Dictionary<string, object?>>>(environmentFile, _jsonSerializerOptions.Value) ?? [];
    }

    private void LoadVariables(Dictionary<string, Dictionary<string, object?>> scopes)
    {
        foreach (var scope in scopes)
        {
            var scopeCollection = GetScopeCollection(scope.Key);
            LoadScopeVariables(scope.Value, scopeCollection);
        }
    }

    private VariablesCollection GetScopeCollection(string scopeName)
        => scopeName switch
        {
            nameof(_variables.GlobalVariables) => _variables.GlobalVariables,
            nameof(_variables.EnvironmentVariables) => _variables.EnvironmentVariables,
            nameof(_variables.CollectionVariables) => _variables.CollectionVariables,
            nameof(_variables.TestCaseVariables) => _variables.TestCaseVariables,
            _ => throw new InvalidOperationException($"Unsupported variables scope with name '{scopeName}'.")
        };

    private static void LoadScopeVariables(Dictionary<string, object?> scope, VariablesCollection variablesCollection)
    {
        foreach (var variable in scope)
        {
            variablesCollection.Set(variable.Key, variable.Value);
        }
    }
}
