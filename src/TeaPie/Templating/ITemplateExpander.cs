namespace TeaPie.Templating;

internal interface ITemplateExpander
{
    TemplateExpansionResult Expand(string content, string filePath);
}
