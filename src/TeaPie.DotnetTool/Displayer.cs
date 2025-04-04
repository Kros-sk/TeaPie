using Spectre.Console;
using System.Reflection;

namespace TeaPie.DotnetTool;

internal static class Displayer
{
    private static readonly Color _teaColor = Color.Teal;
    private static readonly Color _pieColor = new(195, 124, 36);
    private static readonly Color _versionColor = Color.White;

    private static readonly FigletFont _font = FigletFont.Load(FigletFontPath);

    private const string FigletFontPath = "Assets/Fonts/small.flf";
    private const int TeaFigletWidth = 21;
    private const int PieFigletWidth = 17;

    private const string TeaText = "Tea";
    private const string PieText = "Pie";

    public static void DisplayApplicationHeader()
    {
        var table = RenderTable();
        AnsiConsole.Write(table);
    }

    public static Table RenderTable()
    {
        var table = new Table
        {
            Border = TableBorder.None
        };

        GetFiglets(out var figletTea, out var figletPie, out var figletVersion);
        AddFigletsToTable(table, figletTea, figletPie, figletVersion);

        return table;
    }

    private static void AddFigletsToTable(Table table, FigletText figletTea, FigletText figletPie, FigletText figletVersion)
    {
        table.AddColumn(new TableColumn(figletTea).Width(TeaFigletWidth));
        table.AddColumn(new TableColumn(figletPie).Width(PieFigletWidth));
        table.AddColumn(new TableColumn(figletVersion));
        table.Collapse();
    }

    private static void GetFiglets(out FigletText figletTea, out FigletText figletPie, out FigletText figletVersion)
    {
        figletTea = new FigletText(_font, TeaText)
                .Color(_teaColor);
        figletPie = new FigletText(_font, PieText)
                .Color(_pieColor);

        figletVersion = GetFigletWithVersion();
    }

    private static FigletText GetFigletWithVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        var versionText = GetVersionText(version);

        return new FigletText(_font, versionText)
            .Color(_versionColor);
    }

    private static string GetVersionText(Version? version)
        => version is not null
            ? string.Join('.', version.Major, version.Minor, version.Build)
            : string.Empty;
}
