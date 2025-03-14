﻿using Spectre.Console;
using TeaPie.Reporting;

namespace TeaPie.StructureExploration;

internal class SpectreConsoleTreeStructureRenderer : ITreeStructureRenderer
{
    public object Render(IReadOnlyCollectionStructure collectionStructure)
    {
        var folders = collectionStructure.Folders;
        var foldersByParent = GroupFoldersByParent(folders);
        var testCasesByParent = GroupTestCasesByParent(collectionStructure.TestCases);
        var rootFolder = collectionStructure.Root
            ?? throw new InvalidOperationException("Unable to find root of structure.");

        return BuildTree(rootFolder, foldersByParent, testCasesByParent, collectionStructure);
    }

    private static Tree BuildTree(
        Folder rootFolder,
        Dictionary<string, List<Folder>> foldersByParent,
        Dictionary<string, List<TestCase>> testCasesByParent,
        IReadOnlyCollectionStructure collectionStructure)
    {
        var tree = new Tree($"[white]{rootFolder.Name}[/]");
        var queue = new Queue<Node>();
        queue.Enqueue(new Node(rootFolder.RelativePath, tree));

        while (queue.Count > 0)
        {
            var parentNode = queue.Dequeue();
            ResolveEnvironmentFile(collectionStructure.EnvironmentFile, parentNode.RelativePath, parentNode.TreeNode);
            ResolveInitilizationScript(collectionStructure.InitializationScript, parentNode.RelativePath, parentNode.TreeNode);
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

    private static void ResolveInitilizationScript(Script? initializationScript, string parentRelativePath, IHasTreeNodes parentNode)
    {
        if (initializationScript is not null && parentRelativePath.Equals(initializationScript.File.ParentFolder.RelativePath))
        {
            parentNode.AddNode(GetInitializationScriptReport(initializationScript));
        }
    }

    private static void ResolveEnvironmentFile(File? environmentFile, string parentRelativePath, IHasTreeNodes parentNode)
    {
        if (environmentFile is not null && parentRelativePath.Equals(environmentFile.ParentFolder.RelativePath))
        {
            parentNode.AddNode(GetEnvironmentFileReport(environmentFile));
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
        var emoji = CompatibilityChecker.SupportsEmoji ? Emoji.Known.OpenFileFolder : "[grey italic]FO[/]";
        return $"{emoji} [white]{name}[/]";
    }

    private static string GetInitializationScriptReport(Script initializationScript)
    {
        var emoji = CompatibilityChecker.SupportsEmoji ? Emoji.Known.Rocket : "[grey italic]IN[/]";
        return $"{emoji} [aqua]{initializationScript.File.Name}[/]";
    }

    private static string GetEnvironmentFileReport(File environmentFile)
    {
        var emoji = CompatibilityChecker.SupportsEmoji ? Emoji.Known.LeafFlutteringInWind : "[grey italic]EN[/]";
        return $"{emoji} [purple]{environmentFile.Name}[/]";
    }

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
