using TeaPie.Variables;

namespace TeaPie.Templating;

internal sealed class VariablesFluidModelBuilder : IVariablesFluidModelBuilder
{
    public IReadOnlyDictionary<string, object?> Build(IVariables variables)
    {
        var model = new Dictionary<string, object?>();

        foreach (var scope in GetScopesInAscendingPriorityOrder(variables))
        {
            if (scope is null)
            {
                continue;
            }

            foreach (var variable in scope)
            {
                if (variable.HasTag(Constants.SecretVariableTag))
                {
                    model.Remove(variable.Name);
                    continue;
                }

                model[variable.Name] = variable.GetValue<object>();
            }
        }

        return model;
    }

    private static IEnumerable<VariablesCollection> GetScopesInAscendingPriorityOrder(IVariables variables) =>
    [
        variables.GlobalVariables,
        variables.EnvironmentVariables,
        variables.CollectionVariables,
        variables.TestCaseVariables
    ];
}
