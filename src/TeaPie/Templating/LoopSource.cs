using System.Collections;

namespace TeaPie.Templating;

internal readonly record struct LoopSource(IEnumerable? Collection, int ItemCount);
