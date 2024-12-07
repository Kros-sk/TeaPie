namespace TeaPie.Variables;

internal record RequestVariableDescription(string Name, string Type, string Content, string Query)
{
    public override string ToString() => string.Join('.', Name, Type, Content, Query);
}
