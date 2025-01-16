using Spectre.Console;
using TeaPie.Reporting;

namespace TeaPie.StructureExploration;

internal class SpectreConsoleTreeStructureRenderer : ITreeStructureRenderer
{
    private static readonly bool _supportsEmojis = CompatibilityChecker.SupportsEmojis();

    public object Render(IReadOnlyCollectionStructure collectionStructure)
    {
        var folders = collectionStructure.Folders;
        var foldersByParent = GroupFoldersByParent(folders);
        var testCasesByParent = GroupTestCasesByParent(collectionStructure.TestCases);

        var rootFolder = collectionStructure.Root
            ?? throw new InvalidOperationException("Unable to find root of structure.");

        return BuildTree(foldersByParent, testCasesByParent, rootFolder);
    }

    private static Tree BuildTree(
        Dictionary<string, List<Folder>> foldersByParent,
        Dictionary<string, List<TestCase>> testCasesByParent,
        Folder rootFolder)
    {
        var tree = new Tree($"[white]{rootFolder.Name}[/]");
        var queue = new Queue<Node>();
        queue.Enqueue(new Node(rootFolder.RelativePath, tree));

        while (queue.Count > 0)
        {
            var parentNode = queue.Dequeue();
            ResolveFolders(foldersByParent, queue, parentNode.RelativePath, parentNode.TreeNode);
            ResolveTestCases(testCasesByParent, parentNode.RelativePath, parentNode.TreeNode);
        }

        return tree;
    }

    private static void ResolveFolders(
        Dictionary<string, List<Folder>> foldersByParent,
        Queue<Node> queue,
        string parentRelativePath,
        IHasTreeNodes parentNode)
    {
        if (foldersByParent.TryGetValue(parentRelativePath, out var childFolders))
        {
            foreach (var folder in childFolders)
            {
                var childNode = parentNode.AddNode(GetFolderReport(folder.Name.EscapeMarkup()));
                queue.Enqueue(new Node(folder.RelativePath, childNode));
            }
        }
    }

    private static void ResolveTestCases(
        Dictionary<string, List<TestCase>> testCasesByParent,
        string parentRelativePath,
        IHasTreeNodes parentNode)
    {
        if (testCasesByParent.TryGetValue(parentRelativePath, out var childTestCases))
        {
            foreach (var testCase in childTestCases)
            {
                parentNode.AddNode(GetTestCaseReport(testCase));
            }
        }
    }

    private static string GetFolderReport(string name)
    {
        var emoji = _supportsEmojis ? Emoji.Known.OpenFileFolder : "[grey italic]FO[/]";
        return $"{emoji} [white]{name}[/]";
    }

    private static string GetTestCaseReport(TestCase testCase)
    {
        var emoji = _supportsEmojis ? Emoji.Known.TestTube : "[grey italic]TC[/]";
        var name = $"{emoji} [green]{testCase.Name}[/]";

        var descriptionParts = new List<string>();
        if (testCase.PreRequestScripts.Any())
        {
            descriptionParts.Add("init");
        }

        if (testCase.PostResponseScripts.Any())
        {
            descriptionParts.Add("test");
        }

        var description = descriptionParts.Count > 0
            ? $" [grey]({string.Join(", ", descriptionParts)})[/]"
            : string.Empty;

        return name + description;
    }

    private static Dictionary<string, List<Folder>> GroupFoldersByParent(IEnumerable<Folder> folders)
        => folders
            .Where(f => f.ParentFolder is not null)
            .GroupBy(f => f.ParentFolder!.RelativePath)
            .ToDictionary(g => g.Key, g => g.ToList());

    private static Dictionary<string, List<TestCase>> GroupTestCasesByParent(IEnumerable<TestCase> testCases)
        => testCases
            .Where(tc => tc.ParentFolder is not null)
            .GroupBy(tc => tc.ParentFolder.RelativePath)
            .ToDictionary(g => g.Key, g => g.ToList());

    private record Node(string RelativePath, IHasTreeNodes TreeNode);
}
