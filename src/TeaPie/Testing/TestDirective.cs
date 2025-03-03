namespace TeaPie.Testing;

internal record TestDirective(
    string Name,
    string Pattern,
    Func<IReadOnlyDictionary<string, string>, string> TestNameGetter,
    Func<HttpResponseMessage, IReadOnlyDictionary<string, string>, Task> TestFunction);
