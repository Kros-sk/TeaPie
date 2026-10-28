namespace TeaPie.Templating;

internal sealed record LoopBlock(
    string LoopVariableName,
    string SourceExpression,
    string Body,
    int StartIndex,
    int Length,
    int SourceExpressionStartIndex,
    int SourceExpressionRawLength);
