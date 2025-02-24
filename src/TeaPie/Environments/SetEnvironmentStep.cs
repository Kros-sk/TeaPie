using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;
using TeaPie.Variables;

namespace TeaPie.Environments;

internal class SetEnvironmentStep(IVariables variables, IEnvironmentsRegistry environmentsRegistry) : IPipelineStep
{
    private readonly IVariables _variables = variables;
    private readonly IEnvironmentsRegistry _environmentsRegistry = environmentsRegistry;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        if (_environmentsRegistry.IsRegistered(context.EnvironmentName))
        {
            var environment = _environmentsRegistry.Get(context.EnvironmentName);
            environment.Apply(_variables.EnvironmentVariables);
            context.Logger.LogInformation("Running on {EnvironmentName} environment.", context.EnvironmentName);
        }
        else
        {
            context.Logger.LogError("Given environment {EnvironmentName} was not found.", context.EnvironmentName);
            throw new InvalidOperationException("Unable to set non-existing environment.");
        }

        await Task.CompletedTask;
    }
}
