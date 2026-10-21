using FluentAssertions;
using TeaPie.Templating;

namespace TeaPie.Tests.Templating;

public class LoopBlockHierarchyShould
{
    [Fact]
    public void ReportNoAncestorsAndNoDescendantsForAStandaloneBlock()
    {
        const string content = "{% for a in As %}BODY{% endfor %}";
        var blocks = new LoopBlockScanner().FindLoopBlocks(content);

        LoopBlockHierarchy.GetAncestorIndices(0, blocks).Should().BeEmpty();
        LoopBlockHierarchy.IsTopLevel(0, blocks).Should().BeTrue();
        LoopBlockHierarchy.HasDescendant(0, blocks).Should().BeFalse();
        LoopBlockHierarchy.IsNestingRoot(0, blocks).Should().BeFalse();
        LoopBlockHierarchy.IsStandaloneBlock(0, blocks).Should().BeTrue();
    }

    [Fact]
    public void ReportTheOuterBlockAsTheSoleAncestorOfANestedBlock()
    {
        const string content = "{% for outer in Outers %}{% for inner in Inners %}BODY{% endfor %}{% endfor %}";
        var blocks = new LoopBlockScanner().FindLoopBlocks(content);
        var outerIndex = blocks.ToList().FindIndex(b => b.LoopVariableName == "outer");
        var innerIndex = blocks.ToList().FindIndex(b => b.LoopVariableName == "inner");

        LoopBlockHierarchy.GetAncestorIndices(innerIndex, blocks).Should().Equal(outerIndex);
        LoopBlockHierarchy.IsTopLevel(outerIndex, blocks).Should().BeTrue();
        LoopBlockHierarchy.IsTopLevel(innerIndex, blocks).Should().BeFalse();
        LoopBlockHierarchy.HasDescendant(outerIndex, blocks).Should().BeTrue();
        LoopBlockHierarchy.IsNestingRoot(outerIndex, blocks).Should().BeTrue();
        LoopBlockHierarchy.IsNestingRoot(innerIndex, blocks).Should().BeFalse();
        LoopBlockHierarchy.IsStandaloneBlock(outerIndex, blocks).Should().BeFalse();
        LoopBlockHierarchy.IsStandaloneBlock(innerIndex, blocks).Should().BeFalse();
    }

    [Fact]
    public void OrderAncestorsInnermostFirstAcrossThreeLevelsOfNesting()
    {
        const string content =
            "{% for a in As %}{% for b in Bs %}{% for c in Cs %}BODY{% endfor %}{% endfor %}{% endfor %}";
        var blocks = new LoopBlockScanner().FindLoopBlocks(content);
        int IndexOf(string name) => blocks.ToList().FindIndex(b => b.LoopVariableName == name);

        var ancestors = LoopBlockHierarchy.GetAncestorIndices(IndexOf("c"), blocks);

        ancestors.Should().Equal(IndexOf("b"), IndexOf("a"));
    }

    [Fact]
    public void TreatSiblingBlocksAsHavingNoAncestorRelationshipToEachOther()
    {
        const string content = "{% for a in As %}A{% endfor %}mid{% for b in Bs %}B{% endfor %}";
        var blocks = new LoopBlockScanner().FindLoopBlocks(content);

        LoopBlockHierarchy.GetAncestorIndices(0, blocks).Should().BeEmpty();
        LoopBlockHierarchy.GetAncestorIndices(1, blocks).Should().BeEmpty();
        LoopBlockHierarchy.IsNestingRoot(0, blocks).Should().BeFalse();
        LoopBlockHierarchy.IsNestingRoot(1, blocks).Should().BeFalse();
        LoopBlockHierarchy.IsStandaloneBlock(0, blocks).Should().BeTrue();
        LoopBlockHierarchy.IsStandaloneBlock(1, blocks).Should().BeTrue();
    }
}
