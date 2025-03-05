using Spectre.Console;
using TeaPie.Pipelines;
using TeaPie.Reporting;

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
            AnsiConsole.Markup($"[red]Collection on path [/][white]'{context.Path.EscapeMarkup()}'[/]" +
                "[red] is empty - nothing to display.[/]");
        }

        await Task.CompletedTask;
    }

    private void DisplayStructure(ApplicationContext context)
    {
        var tree = _treeRenderer.Render(context.CollectionStructure) as Tree;
        var table = new Table
        {
            Border = TableBorder.Rounded
        };

        if (Path.HasExtension(context.Path))
        {
            table.AddColumn($"[bold yellow]Test-Case - {context.StructureName}[/]");
        }
        else
        {
            table.AddColumn($"[bold yellow]Collection - {context.StructureName}[/] " +
                $"[italic white](Number of test-cases: [/][italic bold yellow]{context.TestCases.Count}[/][italic white])[/]");
        }

        table.AddRow(tree!);
        AnsiConsole.Write(table);
    }
}
