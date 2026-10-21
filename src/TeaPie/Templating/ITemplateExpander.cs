namespace TeaPie.Templating;

internal interface ITemplateExpander
{
    string Expand(string content, string filePath);
}
