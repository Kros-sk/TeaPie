namespace TeaPie.Functions;

internal interface IFunctionsExposer
{
    FunctionsCollection CustomFunctions { get; }
    FunctionsCollection DefaultFunctions { get; }
}
