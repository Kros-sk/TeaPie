using Spectre.Console;
using TeaPie.Pipelines;
using TeaPie.Reporting;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.StructureExploration;

internal class DisplayStructureStep(ITreeStructureRenderer treeRenderer) : IPipelineStep
{
    private readonly ITreeStructureRenderer _treeRenderer = treeRenderer;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        if (context.TestCases.Count > 0)
        {
            DisplayStructure(context);
        }
        else
        {
            DisplayEmptyStructure(context);
        }

        await Task.CompletedTask;
    }

    private static void DisplayEmptyStructure(ApplicationContext context)
    {
        var structureType = context.Path.IsCollectionPath() ? "collection" : "test case";
        AnsiConsole.Markup($"[red]The {structureType} at path [/][white]'{context.Path.EscapeMarkup()}'[/]" +
            "[red] is empty - nothing to display.[/]");
    }

    private void DisplayStructure(ApplicationContext context)
    {
        var tree = _treeRenderer.Render(context.CollectionStructure) as Tree;
        var table = new Table
        {
            Border = TableBorder.Rounded
        };

        if (context.Path.IsCollectionPath())
        {
            table.AddColumn($"[bold yellow]Collection - {context.StructureName}[/] " +
                $"[italic white](Number of test cases: [/][italic bold yellow]{context.TestCases.Count}[/][italic white])[/]");
        }
        else
        {
            table.AddColumn($"[bold yellow]Test Case - {context.StructureName}[/]");
        }

        table.AddRow(tree!);
        AnsiConsole.Write(table);
    }
}
