using TeaPie.StructureExploration;

namespace TeaPie.Reporting;

internal interface ITreeStructureRenderer
{
    object Render(IReadOnlyCollectionStructure collectionStructure);
}
