namespace TeaPie.Templating;

internal interface ICollectionSourceResolver
{
    LoopSource Resolve(string sourceExpression, IReadOnlyDictionary<string, object?> model);
}
