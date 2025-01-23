using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TeaPie.Pipelines;
using TeaPie.Variables;

namespace TeaPie.Environments;

internal class InitializeEnvironmentStep(IVariables variables) : IPipelineStep
{
    private readonly IVariables _variables = variables;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {

        if (!File.Exists(context.EnvironmentFilePath))
        {
            throw new InvalidOperationException($"Environment file on path '{context.EnvironmentFilePath}' was not found.");
        }

        //await using var environmentFile = File.OpenRead(context.EnvironmentFilePath);

        var environmentFile = await File.ReadAllTextAsync(context.EnvironmentFilePath, cancellationToken);

        var environments = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(environmentFile) ?? [];

        if (environments.TryGetValue(Constants.DefaultEnvironmentName, out var globals))
        {
            RegisterVariables(_variables.GlobalVariables, globals);

            if (!context.EnvironmentName.Equals(Constants.DefaultEnvironmentName) &&
                environments.TryGetValue(context.EnvironmentName, out var variables))
            {
                RegisterVariables(_variables.EnvironmentVariables, variables);
            }

            var abc = _variables.EnvironmentVariables.Get<string>("StringVar");
            var abc1 = _variables.EnvironmentVariables.Get<bool>("BooleanVar");
            var abc2 = _variables.EnvironmentVariables.Get<double>("DoubleVar");
            var abc3 = _variables.EnvironmentVariables.Get<long>("IntVar");
            var abc4 = _variables.EnvironmentVariables.Get<List<string>>("ListVar");
        }
        else
        {
            context.Logger.LogWarning("Default environment '{DefaultEnvironment}' was not found in environment " +
                "file on path '{Path}'", Constants.DefaultEnvironmentName, context.EnvironmentFilePath);
        }
    }

    private static void RegisterVariables(VariablesCollection collection, Dictionary<string, object> variables)
    {
        foreach (var variable in variables)
        {
            collection.Set(variable.Key, variable.Value);
        }
    }
}
