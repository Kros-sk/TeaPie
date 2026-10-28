using System.Text.Encodings.Web;
using Fluid;
using Fluid.Ast;

namespace TeaPie.Templating;

internal sealed class TopLevelLoopWrapper : AstRewriter
{
    private readonly Action<int, string> _onLoopRendered;
    private readonly List<string> _loopVariableNames = [];

    private TopLevelLoopWrapper(Action<int, string> onLoopRendered)
    {
        _onLoopRendered = onLoopRendered;
    }

    public static IFluidTemplate Wrap(
        IFluidTemplate template, IReadOnlyList<string> expectedLoopVariableNames, Action<int, string> onLoopRendered)
    {
        var wrapper = new TopLevelLoopWrapper(onLoopRendered);
        var wrapped = wrapper.VisitTemplate(template);

        if (!wrapper._loopVariableNames.SequenceEqual(expectedLoopVariableNames, StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                "Templating error: internal error - the loops found in the template " +
                $"({string.Join(", ", wrapper._loopVariableNames)}) do not match the loops found by the scanner " +
                $"({string.Join(", ", expectedLoopVariableNames)}).");
        }

        return wrapped;
    }

    public override Statement Visit(Statement statement)
    {
        if (statement is not ForStatement loop)
        {
            return base.Visit(statement);
        }

        var loopIndex = _loopVariableNames.Count;
        _loopVariableNames.Add(loop.Identifier);
        return new TopLevelLoopStatement(loop, output => _onLoopRendered(loopIndex, output));
    }

    private sealed class TopLevelLoopStatement(ForStatement loop, Action<string> onRendered) : Statement
    {
        public override async ValueTask<Completion> WriteToAsync(
            TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            using var loopWriter = new StringWriter();
            var completion = await loop.WriteToAsync(loopWriter, encoder, context);
            var output = loopWriter.ToString();

            onRendered(output);
            await writer.WriteAsync(output);

            return completion;
        }
    }
}
