using FluentAssertions;
using TeaPie.StructureExploration;

namespace TeaPie.Tests.StructureExploration;

public class TestCaseShould
{
    private static InternalFile CreateRequestFile(string fileName = "MyTest-req.http")
    {
        var folder = new Folder("/home/user/project", "project", "project");
        return InternalFile.Create($"/home/user/project/{fileName}", folder);
    }

    [Fact]
    public void TrimRequestSuffixAndExtensionFromName()
    {
        var requestFile = CreateRequestFile("MyTest-req.http");

        var testCase = new TestCase(requestFile);

        testCase.Name.Should().Be("MyTest");
    }

    [Fact]
    public void SetParentFolderFromRequestFile()
    {
        var requestFile = CreateRequestFile();

        var testCase = new TestCase(requestFile);

        testCase.ParentFolder.Should().Be(requestFile.ParentFolder);
    }

    [Fact]
    public void SetRequestsFile()
    {
        var requestFile = CreateRequestFile();

        var testCase = new TestCase(requestFile);

        testCase.RequestsFile.Should().Be(requestFile);
    }

    [Fact]
    public void HaveEmptyScriptCollectionsInitially()
    {
        var requestFile = CreateRequestFile();

        var testCase = new TestCase(requestFile);

        testCase.PreRequestScripts.Should().BeEmpty();
        testCase.PostResponseScripts.Should().BeEmpty();
    }
}
