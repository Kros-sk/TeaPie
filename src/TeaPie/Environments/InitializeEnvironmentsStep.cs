using Microsoft.Extensions.Logging;
using System.Text.Json;
using TeaPie.Pipelines;
using TeaPie.Variables;

namespace TeaPie.Environments;

internal class InitializeEnvironmentsStep(
    IPipeline pipeline,
    IVariables variables,
    IEnvironmentsRegistry environmentsRegister) : IPipelineStep
{
    private readonly IVariables _variables = variables;
    private readonly IEnvironmentsRegistry _environmentsRegister = environmentsRegister;
    private readonly IPipeline _pipeline = pipeline;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(context);

        var environments = await ParseEnvironmentFile(context.EnvironmentFilePath);

        RegisterEnvironmentsAndApplyDefault(environments, context.EnvironmentFilePath, context.Logger);
        _pipeline.InsertSteps(this, context.ServiceProvider.GetStep<SetEnvironmentStep>());
    }

    private static async Task<Dictionary<string, Dictionary<string, object>>> ParseEnvironmentFile(string environmentFilePath)
    {
        await using var environmentFile = File.OpenRead(environmentFilePath);
        return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(environmentFile) ?? [];
    }

    private void RegisterEnvironmentsAndApplyDefault(
        Dictionary<string, Dictionary<string, object>> environments,
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

    private bool RegisterEnvironmentAndApplyIfDefault(KeyValuePair<string, Dictionary<string, object>> environmentToken)
    {
        var environment = new Environment(environmentToken.Key, environmentToken.Value);
        _environmentsRegister.RegisterEnvironment(environment);

        if (environment.Name.Equals(Constants.DefaultEnvironmentName))
        {
            environment.Apply(_variables.GlobalVariables);
            return true;
        }

        return false;
    }

    private static void ValidateContext(ApplicationContext context)
    {
        if (!File.Exists(context.EnvironmentFilePath))
        {
            throw new InvalidOperationException($"Environment file on path '{context.EnvironmentFilePath}' was not found.");
        }
    }
}
