using FluentAssertions;
using TeaPie.StructureExploration;
using StructureFile = TeaPie.StructureExploration.File;

namespace TeaPie.Tests.StructureExploration;

public class CollectionStructureShould
{
    [Fact]
    public void Constructor_WithoutRoot_HasNullRoot()
    {
        var structure = new CollectionStructure();

        structure.Root.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithRoot_SetsRootAndAddsFolder()
    {
        var root = new Folder("/root", "root", "root");

        var structure = new CollectionStructure(root);

        structure.Root.Should().Be(root);
        structure.Folders.Should().Contain(root);
    }

    [Fact]
    public void TryAddFolder_AddsFolder()
    {
        var structure = new CollectionStructure();
        var folder = new Folder("/folder", "folder", "folder");

        structure.TryAddFolder(folder).Should().BeTrue();
        structure.TryGetFolder("/folder", out var result).Should().BeTrue();
        result.Should().Be(folder);
    }

    [Fact]
    public void TryAddFolder_ReturnsFalse_ForDuplicate()
    {
        var structure = new CollectionStructure();
        var folder = new Folder("/folder", "folder", "folder");

        structure.TryAddFolder(folder);

        structure.TryAddFolder(folder).Should().BeFalse();
    }

    [Fact]
    public void TryGetFolder_ReturnsTrue_ForExistingFolder()
    {
        var structure = new CollectionStructure();
        var folder = new Folder("/folder", "folder", "folder");
        structure.TryAddFolder(folder);

        structure.TryGetFolder("/folder", out var result).Should().BeTrue();
        result.Should().Be(folder);
    }

    [Fact]
    public void TryGetFolder_ReturnsFalse_ForNonExisting()
    {
        var structure = new CollectionStructure();

        structure.TryGetFolder("/nonexistent", out _).Should().BeFalse();
    }

    [Fact]
    public void TryAddTestCase_AddsTestCaseAndAutoAddsParentFolder()
    {
        var structure = new CollectionStructure();
        var parentFolder = new Folder("/parent", "parent", "parent");
        var requestFile = new InternalFile("/parent/req.http", "parent/req.http", parentFolder);
        var testCase = new TestCase(requestFile);

        structure.TryAddTestCase(testCase).Should().BeTrue();
        structure.TestCases.Should().Contain(testCase);
        structure.TryGetFolder("/parent", out _).Should().BeTrue();
    }

    [Fact]
    public void TryAddTestCase_ReturnsFalse_ForDuplicate()
    {
        var structure = new CollectionStructure();
        var parentFolder = new Folder("/parent", "parent", "parent");
        var requestFile = new InternalFile("/parent/req.http", "parent/req.http", parentFolder);
        var testCase = new TestCase(requestFile);

        structure.TryAddTestCase(testCase);

        structure.TryAddTestCase(testCase).Should().BeFalse();
    }

    [Fact]
    public void SetEnvironmentFile_SetsFile()
    {
        var structure = new CollectionStructure();

        structure.SetEnvironmentFile(new StructureFile("env.json"));

        structure.EnvironmentFile.Should().NotBeNull();
    }

    [Fact]
    public void SetEnvironmentFile_Throws_WhenNull()
    {
        var structure = new CollectionStructure();

        var act = () => structure.SetEnvironmentFile(null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void HasEnvironmentFile_ReturnsTrue_WhenSet()
    {
        var structure = new CollectionStructure();
        structure.SetEnvironmentFile(new StructureFile("env.json"));

        structure.HasEnvironmentFile.Should().BeTrue();
    }

    [Fact]
    public void HasEnvironmentFile_ReturnsFalse_WhenNotSet()
    {
        var structure = new CollectionStructure();

        structure.HasEnvironmentFile.Should().BeFalse();
    }

    [Fact]
    public void SetInitializationScript_SetsScript()
    {
        var structure = new CollectionStructure();

        structure.SetInitializationScript(new Script(new StructureFile("init.csx")));

        structure.InitializationScript.Should().NotBeNull();
    }

    [Fact]
    public void SetInitializationScript_Throws_WhenNull()
    {
        var structure = new CollectionStructure();

        var act = () => structure.SetInitializationScript(null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void HasInitializationScript_ReturnsTrue_WhenSet()
    {
        var structure = new CollectionStructure();
        structure.SetInitializationScript(new Script(new StructureFile("init.csx")));

        structure.HasInitializationScript.Should().BeTrue();
    }

    [Fact]
    public void HasInitializationScript_ReturnsFalse_WhenNotSet()
    {
        var structure = new CollectionStructure();

        structure.HasInitializationScript.Should().BeFalse();
    }
}
