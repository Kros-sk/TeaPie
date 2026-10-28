using TeaPie.StructureExploration;

namespace TeaPie.Templating;

internal interface IExpandedContentDumper
{
    void EnsureDumpsAreOutsideOfTestedCollection(ApplicationContext context);

    void Dump(ApplicationContext context, TestCase testCase, string expandedContent);
}
