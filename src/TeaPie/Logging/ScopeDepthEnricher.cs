using Serilog.Core;
using Serilog.Events;

namespace TeaPie.Logging;

public class ScopeDepthEnricher : ILogEventEnricher
{
    public const string ScopeDepthPropertyName = "ScopeDepth";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (!logEvent.Properties.ContainsKey(ScopeDepthPropertyName))
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(ScopeDepthPropertyName, 0));
        }
    }
}
