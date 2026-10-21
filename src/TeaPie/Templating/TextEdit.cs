using System.Text;

namespace TeaPie.Templating;

internal readonly record struct TextEdit(int Start, int Length, string Replacement);

internal readonly record struct TransformedTextSpan(
    int TransformedStart, int TransformedEnd, int OriginalStart, int OriginalEnd, bool IsEdited);

internal static class TextEditApplier
{
    public static string Apply(string content, IEnumerable<TextEdit> edits) => Apply(content, edits, out _);

    public static string Apply(
        string content, IEnumerable<TextEdit> edits, out IReadOnlyList<TransformedTextSpan> map)
    {
        var ordered = edits.OrderBy(edit => edit.Start).ToList();
        var result = new StringBuilder();
        var spans = new List<TransformedTextSpan>();
        var cursor = 0;

        foreach (var edit in ordered)
        {
            AppendUnchangedSpan(content, result, spans, cursor, edit.Start);

            if (edit.Replacement.Length > 0)
            {
                spans.Add(new TransformedTextSpan(
                    result.Length, result.Length + edit.Replacement.Length,
                    edit.Start, edit.Start + edit.Length, IsEdited: true));
                result.Append(edit.Replacement);
            }

            cursor = edit.Start + edit.Length;
        }

        AppendUnchangedSpan(content, result, spans, cursor, content.Length);

        map = spans;
        return result.ToString();
    }

    private static void AppendUnchangedSpan(
        string content, StringBuilder result, List<TransformedTextSpan> spans, int start, int end)
    {
        if (end <= start)
        {
            return;
        }

        spans.Add(new TransformedTextSpan(result.Length, result.Length + (end - start), start, end, IsEdited: false));
        result.Append(content, start, end - start);
    }
}
