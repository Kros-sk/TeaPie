using TeaPie.Variables;

namespace TeaPie.Templating;

internal interface IVariablesFluidModelBuilder
{
    IReadOnlyDictionary<string, object?> Build(IVariables variables);
}
