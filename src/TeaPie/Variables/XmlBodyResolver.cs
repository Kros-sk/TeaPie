using System.Xml;

namespace TeaPie.Variables;

internal class XmlBodyResolver : IBodyResolver
{
    public bool CanResolve(string mediaType)
        => mediaType.Equals("application/xml", StringComparison.OrdinalIgnoreCase) ||
            mediaType.Equals("text/xml", StringComparison.OrdinalIgnoreCase);

    public string Resolve(string body, string query, string defaultValue = "")
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(body);
        var navigator = xmlDocument.CreateNavigator();
        var node = navigator?.SelectSingleNode(query);

        if (node is null)
        {
            return defaultValue;
        }

        return node.Value;
    }
}
