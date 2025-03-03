namespace TeaPie.Testing;

internal record TestDirective(
    string DirectiveName,
    string DirectivePattern,
    Func<IReadOnlyDictionary<string, string>, string> TestNameGetter,
    Func<HttpResponseMessage, IReadOnlyDictionary<string, string>, Task> TestFunction);
