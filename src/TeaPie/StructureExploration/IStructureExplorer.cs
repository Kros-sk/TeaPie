namespace TeaPie.StructureExploration;

internal interface IStructureExplorer
{
    IReadOnlyCollectionStructure Explore(ApplicationContext applicationContext);
}
