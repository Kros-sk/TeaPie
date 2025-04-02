using Spectre.Console;
using TeaPie.Reporting;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.StructureExploration;

internal class SpectreConsoleTreeStructureRenderer(IPathProvider pathProvider) : ITreeStructureRenderer
{
    private readonly IPathProvider _pathProvider = pathProvider;
    private Node? _rootNode;
    private Node? _teaPieNode;
    private Dictionary<string, List<Folder>> _foldersByParent = [];
    private Dictionary<string, List<TestCase>> _testCasesByParent = [];
    private bool _environmentFileResolved;
    private bool _initializationScriptResolved;

    public object Render(IReadOnlyCollectionStructure collectionStructure)
    {
        var folders = collectionStructure.Folders;
        _foldersByParent = GroupFoldersByParent(folders);
        _testCasesByParent = GroupTestCasesByParent(collectionStructure.TestCases);
        var rootFolder = collectionStructure.Root
            ?? throw new InvalidOperationException("Unable to find root of structure.");

        return BuildTree(rootFolder, collectionStructure);
    }

    private Tree BuildTree(
        Folder rootFolder,
        IReadOnlyCollectionStructure collectionStructure)
    {
        var tree = new Tree($"[white]{rootFolder.Name}[/]");
        var queue = new Queue<Node>();
        var rootNode = new Node(rootFolder.RelativePath, tree);
        _rootNode = rootNode;
        queue.Enqueue(rootNode);

        while (queue.Count > 0)
        {
            var parentNode = queue.Dequeue();
            ResolveInitilizationScript(collectionStructure.InitializationScript, parentNode.RelativePath, parentNode.TreeNode);
            ResolveEnvironmentFile(collectionStructure.EnvironmentFile, parentNode.RelativePath, parentNode.TreeNode);
            ResolveFolders(queue, parentNode.RelativePath, parentNode.TreeNode);
            ResolveTestCases(parentNode.RelativePath, parentNode.TreeNode);
        }

        return tree;
    }

    private void ResolveFolders(
        Queue<Node> queue,
        string parentRelativePath,
        IHasTreeNodes parentNode)
    {
        if (_foldersByParent.TryGetValue(parentRelativePath, out var childFolders))
        {
            foreach (var folder in childFolders)
            {
                ResolveFolder(queue, parentNode, folder);
            }
        }
    }

    private void ResolveFolder(Queue<Node> queue, IHasTreeNodes parentNode, Folder folder)
    {
        var childNode = parentNode.AddNode(GetFolderReport(folder.Name.EscapeMarkup()));
        var node = new Node(folder.RelativePath, childNode);
        queue.Enqueue(node);

        UpdateTeaPieFolderIfNeeded(folder, node);
    }

    private void UpdateTeaPieFolderIfNeeded(Folder folder, Node node)
    {
        if (_teaPieNode is null && folder.Path.Equals(_pathProvider.TeaPieFolderPath))
        {
            _teaPieNode = node;
        }
    }

    private void ResolveInitilizationScript(
        Script? initializationScript, string parentRelativePath, IHasTreeNodes parentNode)
        => ResolveSpecialFile(
            ref _initializationScriptResolved,
            initializationScript?.File,
            parentRelativePath,
            parentNode,
            GetInitializationScriptReport);

    private void ResolveEnvironmentFile(File? environmentFile, string parentRelativePath, IHasTreeNodes parentNode)
        => ResolveSpecialFile(
            ref _environmentFileResolved, environmentFile, parentRelativePath, parentNode, GetEnvironmentFileReport);

    private void ResolveSpecialFile(
        ref bool indicator,
        File? specialFile,
        string parentRelativePath,
        IHasTreeNodes parentNode,
        Func<File, string> reportGetter)
    {
        if (!indicator && specialFile is not null)
        {
            if (File.BelongsTo(specialFile.Path, _pathProvider.RootPath))
            {
                indicator = ResolveInternalFile(indicator, specialFile, parentRelativePath, parentNode, reportGetter);
            }
            else if (_teaPieNode is not null && File.BelongsTo(specialFile.Path, _pathProvider.TeaPieFolderPath))
            {
                indicator = ResolveFileWithinTeaPieFolder(specialFile, reportGetter);
            }
            else if (_rootNode is not null)
            {
                indicator = ResolveRemoteFile(specialFile, reportGetter);
            }
        }
    }

    private static bool ResolveInternalFile(
        bool indicator, File specialFile, string parentRelativePath, IHasTreeNodes parentNode, Func<File, string> reportGetter)
    {
        var envFile = (InternalFile)specialFile;
        if (parentRelativePath.Equals(envFile.ParentFolder.RelativePath))
        {
            parentNode.AddNode(reportGetter(specialFile));
            indicator = true;
        }

        return indicator;
    }

    private bool ResolveFileWithinTeaPieFolder(File specialFile, Func<File, string> reportGetter)
    {
        _teaPieNode!.TreeNode.AddNode(reportGetter(specialFile));
        return true;
    }

    private bool ResolveRemoteFile(File specialFile, Func<File, string> reportGetter)
    {
        _rootNode!.TreeNode.AddNode(reportGetter(specialFile));
        return true;
    }

    private void ResolveTestCases(string parentRelativePath, IHasTreeNodes parentNode)
    {
        if (_testCasesByParent.TryGetValue(parentRelativePath, out var childTestCases))
        {
            foreach (var testCase in childTestCases)
            {
                parentNode.AddNode(GetTestCaseReport(testCase));
            }
        }
    }

    private static string GetFolderReport(string name)
    {
        var emoji = CompatibilityChecker.SupportsEmoji ? Emoji.Known.OpenFileFolder : "[grey italic]FO[/]";
        return $"{emoji} [white]{name}[/]";
    }

    private string GetInitializationScriptReport(File initializationScriptFile)
    {
        var emoji = CompatibilityChecker.SupportsEmoji ? Emoji.Known.Rocket : "[grey italic]IN[/]";
        return GetFileReport("aqua", initializationScriptFile, emoji);
    }

    private string GetEnvironmentFileReport(File environmentFile)
    {
        var emoji = CompatibilityChecker.SupportsEmoji ? Emoji.Known.LeafFlutteringInWind : "[grey italic]EN[/]";
        return GetFileReport("purple", environmentFile, emoji);
    }

    private string GetFileReport(string style, File file, string emoji)
    {
        if (IsInternal(file))
        {
            return $"{emoji} [{style}]" + file.Name + "[/]";
        }
        else
        {
            return $"{emoji} [{style}]" + "[REMOTE] ".EscapeMarkup() + file.Path + "[/]";
        }
    }

    private bool IsInternal(File environmentFile)
        => File.BelongsTo(environmentFile.Path, _pathProvider.RootPath) ||
            File.BelongsTo(environmentFile.Path, _pathProvider.TeaPieFolderPath);

    private static string GetTestCaseReport(TestCase testCase)
    {
        var emoji = CompatibilityChecker.SupportsEmoji ? Emoji.Known.TestTube : "[grey italic]TC[/]";
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
