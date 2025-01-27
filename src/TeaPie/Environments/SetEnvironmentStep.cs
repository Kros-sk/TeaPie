using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;
using TeaPie.Variables;

namespace TeaPie.Environments;

internal class SetEnvironmentStep(IVariables variables, IEnvironmentsRegistry environmentsRegister) : IPipelineStep
{
    private readonly IVariables _variables = variables;
    private readonly IEnvironmentsRegistry _environmentsRegister = environmentsRegister;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        if (_environmentsRegister.TryGetEnvironment(context.EnvironmentName, out var environment))
        {
            environment.Apply(_variables.EnvironmentVariables);
        }
        else
        {
            context.Logger.LogError("Given environment {EnvironmentName} was not found.", context.EnvironmentName);
            throw new InvalidOperationException("Unable to set non-existing environment.");
        }

        await Task.CompletedTask;
    }
}
