using TeaPie.StructureExploration;

namespace TeaPie.Scripts;

internal class ScriptPreProcessContext(Script script, List<ScriptReference> referencedScripts)
{
    public Script Script { get; set; } = script;

    public IReadOnlyList<ScriptReference> ReferencedScripts => _referencedScripts;

    private readonly List<ScriptReference> _referencedScripts = referencedScripts;

    public void AddScriptReference(ScriptReference reference) => _referencedScripts.Add(reference);
}
