using TeaPie.Json;
using static Xunit.Assert;

namespace TeaPie.Testing;

public static class XunitAssertExtensions
{
    /// <summary>
    /// Verifies whether a JSON object (<paramref name="container"/>) contains another JSON object
    /// (<paramref name="contained"/>). If not, assertion exception is thrown.
    /// Property names specified in <paramref name="ignoreProperties"/> are excluded from the comparison.
    /// </summary>
    /// <param name="container">The JSON string expected to contain the <paramref name="contained"/> JSON.</param>
    /// <param name="contained">The JSON string expected to be contained within the <paramref name="container"/> JSON.</param>
    /// <param name="ignoreProperties">An array of property names to exclude from the comparison.</param>
    public static void JsonContains(string container, string contained, params string[] ignoreProperties)
    {
        if (!JsonHelper.JsonContains(container, contained, out var error, ignoreProperties))
        {
            Fail("The provided JSON does not contain the expected JSON." + Environment.NewLine +
                 $"Error: Expected {error.Value.expected}, but found {error.Value.found}.");
        }
    }
}
