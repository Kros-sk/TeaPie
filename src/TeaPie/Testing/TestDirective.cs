namespace TeaPie.Testing;

internal record TestDirective(
    string DirectiveName,
    string DirectivePattern,
    Func<IReadOnlyDictionary<string, object>, string> TestNameGetter,
    Func<HttpResponseMessage, IReadOnlyDictionary<string, object>, Task> TestFunction);
