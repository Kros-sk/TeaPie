using Microsoft.Extensions.Logging;
using System.Text.Json;
using TeaPie.Json;
using TeaPie.Pipelines;
using TeaPie.Variables;

namespace TeaPie.Environments;

internal class InitializeEnvironmentsStep(
    IPipeline pipeline,
    IVariables variables,
    IEnvironmentsRegistry environmentsRegistry) : IPipelineStep
{
    private readonly IVariables _variables = variables;
    private readonly IEnvironmentsRegistry _environmentsRegistry = environmentsRegistry;
    private readonly IPipeline _pipeline = pipeline;

    private static readonly Lazy<JsonSerializerOptions> _jsonSerializerOptions = new(() =>
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonElementTypeConverter());
        return options;
    });

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        if (context.CollectionStructure.HasEnvironmentFile)
        {
            await ResolveEnvironments(context);
            _pipeline.InsertSteps(this, context.ServiceProvider.GetStep<SetEnvironmentStep>());
        }
        else
        {
            context.Logger.LogWarning("No environment file found. Running without environment.");
        }
    }

    private async Task ResolveEnvironments(ApplicationContext context)
    {
        ValidateContext(context);

        var environments = await ParseEnvironmentFile(context.EnvironmentFilePath);

        EnsureEnvironmentNameIsSet(context);
        RegisterEnvironmentsAndApplyDefault(environments, context.EnvironmentFilePath, context.Logger);
    }

    private static async Task<Dictionary<string, Dictionary<string, object?>>> ParseEnvironmentFile(string environmentFilePath)
    {
        await using var environmentFile = File.OpenRead(environmentFilePath);
        return JsonSerializer
            .Deserialize<Dictionary<string, Dictionary<string, object?>>>(environmentFile, _jsonSerializerOptions.Value) ?? [];
    }

    private void RegisterEnvironmentsAndApplyDefault(
        Dictionary<string, Dictionary<string, object?>> environments,
        string environmentFilePath,
        ILogger logger)
    {
        var defaultApplied = false;
        foreach (var environmentToken in environments)
        {
            defaultApplied |= RegisterEnvironmentAndApplyIfDefault(environmentToken);
        }

        if (!defaultApplied)
        {
            logger.LogWarning("Default environment '{DefaultEnvironment}' was not found in the environment " +
                "file on path '{Path}'", Constants.DefaultEnvironmentName, environmentFilePath);
        }
    }

    private bool RegisterEnvironmentAndApplyIfDefault(KeyValuePair<string, Dictionary<string, object?>> environmentToken)
    {
        var environment = new Environment(environmentToken.Key, environmentToken.Value);
        _environmentsRegistry.RegisterEnvironment(environment);

        if (environment.Name.Equals(Constants.DefaultEnvironmentName))
        {
            environment.Apply(_variables.GlobalVariables);
            return true;
        }

        return false;
    }

    private static void EnsureEnvironmentNameIsSet(ApplicationContext context)
    {
        if (context.EnvironmentName.Equals(string.Empty))
        {
            context.EnvironmentName = Constants.DefaultEnvironmentName;
        }
    }

    private static void ValidateContext(ApplicationContext context)
    {
        if (!File.Exists(context.EnvironmentFilePath))
        {
            throw new InvalidOperationException($"Environment file on path '{context.EnvironmentFilePath}' was not found.");
        }
    }
}
